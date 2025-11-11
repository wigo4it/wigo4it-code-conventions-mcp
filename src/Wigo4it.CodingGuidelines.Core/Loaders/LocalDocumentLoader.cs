using System.Text.RegularExpressions;
using Wigo4it.CodingGuidelines.Core.Configuration;
using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Loaders;

/// <summary>
/// Loads documents from local file system
/// </summary>
public class LocalDocumentLoader : IDocumentLoader
{
    private readonly DocumentSourceConfiguration _configuration;

    public LocalDocumentLoader(DocumentSourceConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task<List<Document>> LoadDocumentsAsync()
    {
        var documents = new List<Document>();

        if (string.IsNullOrEmpty(_configuration.LocalBasePath))
        {
            return documents;
        }

        var docsPath = Path.Combine(_configuration.LocalBasePath, _configuration.DocsPath);
        
        if (!Directory.Exists(docsPath))
        {
            return documents;
        }

        var markdownFiles = Directory.GetFiles(docsPath, "*.md", SearchOption.AllDirectories);

        foreach (var filePath in markdownFiles)
        {
            try
            {
                var content = await File.ReadAllTextAsync(filePath);
                var relativePath = Path.GetRelativePath(docsPath, filePath).Replace("\\", "/");
                var document = ParseDocument(content, relativePath, filePath);
                
                if (document != null)
                {
                    documents.Add(document);
                }
            }
            catch (Exception ex)
            {
                // Log error and continue with next file
                Console.Error.WriteLine($"Error loading document {filePath}: {ex.Message}");
            }
        }

        return documents;
    }

    public async Task<Document?> GetDocumentByPathAsync(string path)
    {
        if (string.IsNullOrEmpty(_configuration.LocalBasePath))
        {
            return null;
        }

        var docsPath = Path.Combine(_configuration.LocalBasePath, _configuration.DocsPath);
        var filePath = Path.Combine(docsPath, path);

        if (!File.Exists(filePath))
        {
            return null;
        }

        try
        {
            var content = await File.ReadAllTextAsync(filePath);
            var relativePath = Path.GetRelativePath(docsPath, filePath).Replace("\\", "/");
            return ParseDocument(content, relativePath, filePath);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error loading document {filePath}: {ex.Message}");
            return null;
        }
    }

    private static Document? ParseDocument(string content, string relativePath, string filePath)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var directory = Path.GetDirectoryName(relativePath)?.Replace("\\", "/") ?? "";
        
        // Extract title from first heading or filename
        var title = ExtractTitle(content) ?? fileName;
        
        // Determine document type from directory structure
        var type = DetermineDocumentType(directory);
        
        // Extract metadata
        var category = ExtractCategory(directory, content);
        var language = ExtractLanguage(content, fileName);
        var tags = ExtractTags(content);

        var document = new Document
        {
            Id = GenerateId(relativePath),
            Title = title,
            Content = content,
            Type = type,
            Path = relativePath,
            Category = category,
            Language = language,
            Tags = tags,
            LastModified = File.GetLastWriteTime(filePath)
        };

        return document;
    }

    private static string ExtractTitle(string content)
    {
        // Look for first # heading
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
        // Try to extract category from directory structure
        var parts = directory.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length > 1)
        {
            return parts[^1]; // Last directory as category
        }

        // Look for category in frontmatter or metadata
        var match = Regex.Match(content, @"(?:Category|category):\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static string? ExtractLanguage(string content, string fileName)
    {
        // Check common language indicators in filename
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

        // Look for language in frontmatter or metadata
        var match = Regex.Match(content, @"(?:Language|language):\s*(.+)$", RegexOptions.Multiline);
        return match.Success ? match.Groups[1].Value.Trim() : null;
    }

    private static List<string> ExtractTags(string content)
    {
        // Look for tags in frontmatter or metadata
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
        // Generate a consistent ID from the path
        return relativePath.Replace("/", "-").Replace(".md", "").Replace(" ", "-");
    }
}
