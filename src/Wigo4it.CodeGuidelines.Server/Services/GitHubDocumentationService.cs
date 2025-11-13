using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;

namespace Wigo4it.CodeGuidelines.Server.Services;

/// <summary>
/// GitHub-based implementation of the documentation service.
/// Fetches documentation from a GitHub repository using the GitHub API.
/// </summary>
public sealed class GitHubDocumentationService : IDocumentationService
{
    private readonly DocumentationOptions _options;
    private readonly ILogger<GitHubDocumentationService> _logger;
    private readonly HttpClient _httpClient;
    private readonly ConcurrentDictionary<string, DocumentationMetadata> _cache = new();
    private readonly ConcurrentDictionary<string, string> _contentCache = new();
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public GitHubDocumentationService(
        IOptions<DocumentationOptions> options,
        IHttpClientFactory httpClientFactory,
        ILogger<GitHubDocumentationService> logger)
    {
        _options = options.Value;
        _logger = logger;
        _httpClient = httpClientFactory.CreateClient("GitHub");
        
        // Configure HTTP client for GitHub API
        _httpClient.BaseAddress = new Uri("https://api.github.com");
        _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/vnd.github+json"));
        _httpClient.DefaultRequestHeaders.UserAgent.Add(new ProductInfoHeaderValue("Wigo4it-CodeGuidelines-MCP", "1.0"));
        
        if (!string.IsNullOrWhiteSpace(_options.GitHubToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _options.GitHubToken);
        }
    }

    public async Task<IReadOnlyList<DocumentationMetadata>> GetAllDocumentationAsync()
    {
        await EnsureInitializedAsync();
        return _cache.Values.ToList();
    }

    public async Task<IReadOnlyList<DocumentationMetadata>> GetDocumentationByCategoryAsync(DocumentationCategory category)
    {
        await EnsureInitializedAsync();
        return _cache.Values
            .Where(doc => doc.Category == category)
            .ToList();
    }

    public async Task<DocumentationContent?> GetDocumentationContentAsync(string id)
    {
        await EnsureInitializedAsync();

        if (!_cache.TryGetValue(id, out var metadata))
        {
            _logger.LogWarning("Documentation with ID '{Id}' not found", id);
            return null;
        }

        // Check content cache first
        if (_contentCache.TryGetValue(id, out var cachedContent))
        {
            return new DocumentationContent
            {
                Metadata = metadata,
                Content = cachedContent
            };
        }

        try
        {
            // Fetch content from GitHub
            var content = await FetchFileContentFromGitHubAsync(metadata.FilePath);
            
            // Cache the content
            _contentCache.TryAdd(id, content);
            
            return new DocumentationContent
            {
                Metadata = metadata,
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to fetch documentation from GitHub: {FilePath}", metadata.FilePath);
            return null;
        }
    }

    private async Task EnsureInitializedAsync()
    {
        if (_isInitialized)
        {
            return;
        }

        await _initLock.WaitAsync();
        try
        {
            if (_isInitialized)
            {
                return;
            }

            await InitializeDocumentationCacheAsync();
            _isInitialized = true;
        }
        finally
        {
            _initLock.Release();
        }
    }

    private async Task InitializeDocumentationCacheAsync()
    {
        _logger.LogInformation(
            "Initializing documentation cache from GitHub: {Owner}/{Repo}@{Branch}/{DocsPath}",
            _options.GitHubOwner,
            _options.GitHubRepository,
            _options.GitHubBranch,
            _options.DocsPath);

        // Scan each category folder
        foreach (var category in Enum.GetValues<DocumentationCategory>())
        {
            var categoryPath = $"{_options.DocsPath}/{category}";
            await ScanCategoryFolderAsync(categoryPath, category);
        }

        _logger.LogInformation("Initialized {Count} documentation files from GitHub", _cache.Count);
    }

    private async Task ScanCategoryFolderAsync(string categoryPath, DocumentationCategory category)
    {
        try
        {
            var files = await ListFilesInDirectoryAsync(categoryPath);
            
            foreach (var file in files.Where(f => f.Name.EndsWith(".md", StringComparison.OrdinalIgnoreCase)))
            {
                try
                {
                    var metadata = await CreateMetadataFromGitHubFileAsync(file, category);
                    _cache.TryAdd(metadata.Id, metadata);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to process GitHub file: {Path}", file.Path);
                }
            }
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            _logger.LogDebug("Category folder not found on GitHub: {CategoryPath}", categoryPath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scan GitHub category folder: {CategoryPath}", categoryPath);
        }
    }

    private async Task<List<GitHubContentItem>> ListFilesInDirectoryAsync(string path)
    {
        var url = $"/repos/{_options.GitHubOwner}/{_options.GitHubRepository}/contents/{path}?ref={_options.GitHubBranch}";
        
        var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        var items = JsonSerializer.Deserialize<List<GitHubContentItem>>(json);
        
        return items ?? [];
    }

    private async Task<DocumentationMetadata> CreateMetadataFromGitHubFileAsync(GitHubContentItem file, DocumentationCategory category)
    {
        var fileName = Path.GetFileNameWithoutExtension(file.Name);
        var id = $"{category}/{fileName}".ToLowerInvariant();

        var title = fileName;
        var description = string.Empty;

        try
        {
            // Fetch the file content to extract title and description
            var content = await FetchFileContentFromGitHubAsync(file.Path);
            var lines = content.Split('\n');
            
            if (lines.Length > 0)
            {
                // Look for markdown title (# Title)
                var titleLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("# "));
                if (titleLine != null)
                {
                    title = titleLine.TrimStart('#').Trim();
                }

                // Look for description in first paragraph after title
                var descLines = lines
                    .SkipWhile(l => l.TrimStart().StartsWith("#") || string.IsNullOrWhiteSpace(l))
                    .TakeWhile(l => !string.IsNullOrWhiteSpace(l))
                    .Take(3);
                description = string.Join(" ", descLines).Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract title/description from GitHub file: {Path}", file.Path);
        }

        return new DocumentationMetadata
        {
            Id = id,
            Title = title,
            Category = category,
            FilePath = file.Path,
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };
    }

    private async Task<string> FetchFileContentFromGitHubAsync(string path)
    {
        // Use the raw content API endpoint for better performance
        var rawUrl = $"https://raw.githubusercontent.com/{_options.GitHubOwner}/{_options.GitHubRepository}/{_options.GitHubBranch}/{path}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, rawUrl);
        if (!string.IsNullOrWhiteSpace(_options.GitHubToken))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.GitHubToken);
        }
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }

    private sealed class GitHubContentItem
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("path")]
        public string Path { get; set; } = string.Empty;

        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("download_url")]
        public string? DownloadUrl { get; set; }
    }
}
