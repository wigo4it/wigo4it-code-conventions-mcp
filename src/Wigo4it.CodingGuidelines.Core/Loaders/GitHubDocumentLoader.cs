using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using Wigo4it.CodingGuidelines.Core.Configuration;
using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Loaders;

/// <summary>
/// Loads documents from GitHub repository
/// </summary>
public class GitHubDocumentLoader : IDocumentLoader
{
    private readonly HttpClient _httpClient;
    private readonly DocumentSourceConfiguration _configuration;

    /// <summary>
    /// Initializes a new instance of the GitHubDocumentLoader class
    /// </summary>
    /// <param name="httpClient">The HTTP client for GitHub API requests</param>
    /// <param name="configuration">The document source configuration</param>
    public GitHubDocumentLoader(HttpClient httpClient, DocumentSourceConfiguration configuration)
    {
        _httpClient = httpClient;
        _configuration = configuration;
    }

    /// <summary>
    /// Loads all documents from the GitHub repository
    /// </summary>
    /// <returns>List of documents loaded from GitHub</returns>
    public async Task<List<Document>> LoadDocumentsAsync()
    {
        var documents = new List<Document>();

        try
        {
            // Get repository contents for docs folder
            // Note: GitHub API works better without the ref parameter for the default branch
            var apiUrl = $"https://api.github.com/repos/{_configuration.GitHubOwner}/{_configuration.GitHubRepo}/contents/{_configuration.DocsPath}";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Wigo4it-MCP-Server");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

            var files = await GetRepositoryFilesRecursiveAsync(apiUrl);

            foreach (var file in files.Where(f => f.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var content = await DownloadFileContentAsync(file.DownloadUrl);
                    var document = ParseDocument(content, file.Path, file.Name);
                    
                    if (document != null)
                    {
                        documents.Add(document);
                    }
                }
                catch (Exception ex)
                {
                    Console.Error.WriteLine($"Error loading document {file.Path}: {ex.Message}");
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading documents from GitHub: {ex.Message}");
        }

        return documents;
    }

    /// <summary>
    /// Gets a document by its relative path from GitHub
    /// </summary>
    /// <param name="path">The relative path to the document</param>
    /// <returns>The document or null if not found</returns>
    public async Task<Document?> GetDocumentByPathAsync(string path)
    {
        try
        {
            var fullPath = $"{_configuration.DocsPath}/{path}";
            // Note: GitHub API works better without the ref parameter for the default branch
            var apiUrl = $"https://api.github.com/repos/{_configuration.GitHubOwner}/{_configuration.GitHubRepo}/contents/{fullPath}";
            
            _httpClient.DefaultRequestHeaders.Clear();
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Wigo4it-MCP-Server");
            _httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github.v3+json");

            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return null;
            }

            var fileInfo = await response.Content.ReadFromJsonAsync<GitHubFileInfo>();
            
            if (fileInfo == null || string.IsNullOrEmpty(fileInfo.DownloadUrl))
            {
                return null;
            }

            var content = await DownloadFileContentAsync(fileInfo.DownloadUrl);
            return ParseDocument(content, path, fileInfo.Name);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading document {path}: {ex.Message}");
            return null;
        }
    }

    private async Task<List<GitHubFileInfo>> GetRepositoryFilesRecursiveAsync(string apiUrl)
    {
        var allFiles = new List<GitHubFileInfo>();

        try
        {
            var response = await _httpClient.GetAsync(apiUrl);
            
            if (!response.IsSuccessStatusCode)
            {
                return allFiles;
            }

            var items = await response.Content.ReadFromJsonAsync<List<GitHubFileInfo>>();
            
            if (items == null)
            {
                return allFiles;
            }

            foreach (var item in items)
            {
                if (item.Type == "file")
                {
                    allFiles.Add(item);
                }
                else if (item.Type == "dir")
                {
                    // Recursively get files from subdirectory
                    var subFiles = await GetRepositoryFilesRecursiveAsync(item.Url);
                    allFiles.AddRange(subFiles);
                }
            }
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error getting repository files: {ex.Message}");
        }

        return allFiles;
    }

    private async Task<string> DownloadFileContentAsync(string downloadUrl)
    {
        var response = await _httpClient.GetAsync(downloadUrl);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }

    private static Document? ParseDocument(string content, string path, string fileName)
    {
        var pathWithoutDocs = path.StartsWith("docs/") ? path.Substring(5) : path;
        var directory = Path.GetDirectoryName(pathWithoutDocs)?.Replace("\\", "/") ?? "";
        
        // Extract title from first heading or filename
        var title = ExtractTitle(content) ?? Path.GetFileNameWithoutExtension(fileName);
        
        // Determine document type from directory structure
        var type = DetermineDocumentType(directory);
        
        // Extract metadata
        var category = ExtractCategory(directory, content);
        var language = ExtractLanguage(content, fileName);
        var tags = ExtractTags(content);

        var document = new Document
        {
            Id = GenerateId(pathWithoutDocs),
            Title = title,
            Content = content,
            Type = type,
            Path = pathWithoutDocs,
            Category = category,
            Language = language,
            Tags = tags,
            LastModified = null // GitHub API doesn't provide this in contents endpoint
        };

        return document;
    }

    private static string ExtractTitle(string content)
    {
        var match = Regex.Match(content, @"^#\s+(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : string.Empty;
    }

    private static string DetermineDocumentType(string directory)
    {
        var lowerDir = directory.ToLowerInvariant();
        
        if (lowerDir.Contains("adr") || lowerDir.Contains("architecture"))
            return "ADR";
        if (lowerDir.Contains("style") || lowerDir.Contains("styles"))
            return "StyleGuide";
        if (lowerDir.Contains("guideline") || lowerDir.Contains("guidelines"))
            return "CodingGuideline";
        if (lowerDir.Contains("recommendation") || lowerDir.Contains("recommendations"))
            return "Recommendation";
        
        return "Document";
    }

    private static string? ExtractCategory(string directory, string content)
    {
        var parts = directory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 0)
        {
            return parts[^1];
        }

        var match = Regex.Match(content, @"(?:Category|category):\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractLanguage(string content, string fileName)
    {
        var lowerFileName = fileName.ToLowerInvariant();
        if (lowerFileName.Contains("csharp") || lowerFileName.Contains("c-sharp"))
            return "C#";
        if (lowerFileName.Contains("typescript"))
            return "TypeScript";
        if (lowerFileName.Contains("javascript"))
            return "JavaScript";
        if (lowerFileName.Contains("python"))
            return "Python";
        if (lowerFileName.Contains("java") && !lowerFileName.Contains("javascript"))
            return "Java";

        var match = Regex.Match(content, @"(?:Language|language):\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static List<string> ExtractTags(string content)
    {
        var match = Regex.Match(content, @"(?:Tags|tags):\s*(.+)$", RegexOptions.Multiline);
        if (match.Success)
        {
            var tagsString = match.Groups[1].Value.Trim();
            return tagsString.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(t => t.Trim())
                .ToList();
        }

        return new List<string>();
    }

    private static string GenerateId(string relativePath)
    {
        return relativePath.Replace("/", "-").Replace(".md", "").Replace(" ", "-");
    }

    private class GitHubFileInfo
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("url")]
        public string Url { get; set; } = string.Empty;

        [JsonPropertyName("download_url")]
        public string DownloadUrl { get; set; } = string.Empty;
    }
}
