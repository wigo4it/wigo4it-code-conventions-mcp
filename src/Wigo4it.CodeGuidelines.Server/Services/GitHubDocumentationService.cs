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
        var tags = new List<string>();

        try
        {
            // Fetch the file content to extract title, description, and tags
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

            // Extract tags from content
            tags = ExtractTagsFromContentAsync(content);
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
            Description = string.IsNullOrWhiteSpace(description) ? null : description,
            Tags = tags
        };
    }

    private async Task<string> FetchFileContentFromGitHubAsync(string path)
    {
        // Use the raw content API endpoint for better performance
        var rawUrl = $"https://raw.githubusercontent.com/{_options.GitHubOwner}/{_options.GitHubRepository}/{_options.GitHubBranch}/{path}";
        
        var request = new HttpRequestMessage(HttpMethod.Get, rawUrl);
        
        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        return await response.Content.ReadAsStringAsync();
    }


    public async Task<IReadOnlyList<DocumentationSearchResult>> SearchDocumentationAsync(string searchTerm)
    {
        await EnsureInitializedAsync();

        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return [];
        }

        var results = new List<DocumentationSearchResult>();
        var normalizedSearchTerm = searchTerm.ToLowerInvariant();

        foreach (var metadata in _cache.Values)
        {
            try
            {
                var content = await GetOrFetchContentAsync(metadata);
                var searchResult = CalculateSearchRelevance(metadata, content, normalizedSearchTerm);
                
                if (searchResult.MatchCount > 0)
                {
                    results.Add(searchResult);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to search in GitHub file: {FilePath}", metadata.FilePath);
            }
        }

        // Sort by relevance score descending
        return results.OrderByDescending(r => r.RelevanceScore).ToList();
    }

    public async Task<IReadOnlyList<DocumentationMetadata>> GetRelatedDocumentationAsync(string documentId, int maxResults = 5)
    {
        await EnsureInitializedAsync();

        if (!_cache.TryGetValue(documentId, out var sourceDoc))
        {
            _logger.LogWarning("Source documentation with ID '{Id}' not found", documentId);
            return [];
        }

        try
        {
            var sourceContent = await GetOrFetchContentAsync(sourceDoc);
            var sourceKeywords = ExtractKeywords(sourceContent);
            
            var relatedDocs = new List<(DocumentationMetadata Metadata, int Score)>();

            foreach (var metadata in _cache.Values)
            {
                if (metadata.Id == documentId)
                {
                    continue; // Skip the source document itself
                }

                try
                {
                    var targetContent = await GetOrFetchContentAsync(metadata);
                    var targetKeywords = ExtractKeywords(targetContent);
                    
                    var similarity = CalculateSimilarityScore(sourceDoc, sourceKeywords, metadata, targetKeywords);
                    
                    if (similarity > 0)
                    {
                        relatedDocs.Add((metadata, similarity));
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to analyze GitHub file for related docs: {FilePath}", metadata.FilePath);
                }
            }

            return relatedDocs
                .OrderByDescending(d => d.Score)
                .Take(maxResults)
                .Select(d => d.Metadata)
                .ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to find related documentation for: {DocumentId}", documentId);
            return [];
        }
    }

    public async Task<IReadOnlyList<DocumentationMetadata>> GetDocumentationByTagsAsync(IEnumerable<string> tags)
    {
        await EnsureInitializedAsync();

        var normalizedTags = tags.Select(t => t.ToLowerInvariant()).ToHashSet();
        if (normalizedTags.Count == 0)
        {
            return [];
        }

        return _cache.Values
            .Where(doc => doc.Tags.Any(tag => normalizedTags.Contains(tag.ToLowerInvariant())))
            .ToList();
    }

    private async Task<string> GetOrFetchContentAsync(DocumentationMetadata metadata)
    {
        if (_contentCache.TryGetValue(metadata.Id, out var cachedContent))
        {
            return cachedContent;
        }

        var content = await FetchFileContentFromGitHubAsync(metadata.FilePath);
        _contentCache.TryAdd(metadata.Id, content);
        return content;
    }

    private static List<string> ExtractTagsFromContentAsync(string content)
    {
        var tags = new List<string>();
        var lines = content.Split('\n');
        
        // Check for YAML frontmatter
        if (lines.Length > 0 && lines[0].Trim() == "---")
        {
            var frontmatterEnd = Array.FindIndex(lines, 1, l => l.Trim() == "---");
            if (frontmatterEnd > 0)
            {
                for (int i = 1; i < frontmatterEnd; i++)
                {
                    var line = lines[i].Trim();
                    if (line.StartsWith("tags:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Handle inline tags: tags: [tag1, tag2, tag3]
                        var tagContent = line.Substring(5).Trim();
                        if (tagContent.StartsWith('[') && tagContent.EndsWith(']'))
                        {
                            tagContent = tagContent[1..^1];
                            tags.AddRange(tagContent.Split(',').Select(t => t.Trim().Trim('"', '\'')));
                        }
                    }
                    else if (line.StartsWith("- ", StringComparison.Ordinal) && i > 1 && 
                             lines[i - 1].Trim().StartsWith("tags:", StringComparison.OrdinalIgnoreCase))
                    {
                        // Handle list format tags
                        tags.Add(line[2..].Trim().Trim('"', '\''));
                    }
                }
            }
        }

        return tags;
    }

    private DocumentationSearchResult CalculateSearchRelevance(
        DocumentationMetadata metadata,
        string content,
        string normalizedSearchTerm)
    {
        var matchCount = 0;
        var excerpts = new List<string>();
        var titleMatches = 0;
        var descriptionMatches = 0;

        // Check title
        if (metadata.Title.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase))
        {
            titleMatches = CountOccurrences(metadata.Title.ToLowerInvariant(), normalizedSearchTerm);
            matchCount += titleMatches;
        }

        // Check description
        if (!string.IsNullOrEmpty(metadata.Description) &&
            metadata.Description.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase))
        {
            descriptionMatches = CountOccurrences(metadata.Description.ToLowerInvariant(), normalizedSearchTerm);
            matchCount += descriptionMatches;
        }

        // Check tags
        var tagMatches = metadata.Tags.Count(tag => 
            tag.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase));
        matchCount += tagMatches;

        // Check content and extract excerpts
        var contentLines = content.Split('\n');
        var contentMatches = 0;
        
        foreach (var line in contentLines)
        {
            if (line.Contains(normalizedSearchTerm, StringComparison.OrdinalIgnoreCase))
            {
                contentMatches += CountOccurrences(line.ToLowerInvariant(), normalizedSearchTerm);
                
                if (excerpts.Count < 3) // Limit to 3 excerpts
                {
                    var excerpt = line.Trim();
                    if (excerpt.Length > 200)
                    {
                        var index = excerpt.ToLowerInvariant().IndexOf(normalizedSearchTerm);
                        var start = Math.Max(0, index - 100);
                        var length = Math.Min(200, excerpt.Length - start);
                        excerpt = (start > 0 ? "..." : "") + 
                                  excerpt.Substring(start, length) + 
                                  (start + length < excerpt.Length ? "..." : "");
                    }
                    excerpts.Add(excerpt);
                }
            }
        }
        matchCount += contentMatches;

        // Calculate relevance score (0-100)
        // Title matches are weighted highest, then description, then tags, then content
        var relevanceScore = Math.Min(100, 
            (titleMatches * 30) + 
            (descriptionMatches * 20) + 
            (tagMatches * 15) + 
            (contentMatches * 1));

        return new DocumentationSearchResult
        {
            Metadata = metadata,
            RelevanceScore = relevanceScore,
            MatchingExcerpts = excerpts,
            MatchCount = matchCount
        };
    }

    private static int CountOccurrences(string text, string searchTerm)
    {
        var count = 0;
        var index = 0;
        
        while ((index = text.IndexOf(searchTerm, index, StringComparison.Ordinal)) != -1)
        {
            count++;
            index += searchTerm.Length;
        }
        
        return count;
    }

    private static HashSet<string> ExtractKeywords(string content)
    {
        // Extract meaningful words (longer than 3 chars, exclude common words)
        var commonWords = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "the", "and", "for", "are", "but", "not", "you", "all", "can", "had", "her", 
            "was", "one", "our", "out", "day", "get", "has", "him", "his", "how", "man",
            "new", "now", "old", "see", "two", "way", "who", "boy", "did", "its", "let",
            "put", "say", "she", "too", "use", "this", "that", "with", "have", "from", "they"
        };

        var words = System.Text.RegularExpressions.Regex
            .Split(content.ToLowerInvariant(), @"\W+")
            .Where(w => w.Length > 3 && !commonWords.Contains(w))
            .ToHashSet();

        return words;
    }

    private static int CalculateSimilarityScore(
        DocumentationMetadata sourceDoc,
        HashSet<string> sourceKeywords,
        DocumentationMetadata targetDoc,
        HashSet<string> targetKeywords)
    {
        var score = 0;

        // Same category bonus
        if (sourceDoc.Category == targetDoc.Category)
        {
            score += 20;
        }

        // Shared tags bonus
        var sharedTags = sourceDoc.Tags.Intersect(targetDoc.Tags, StringComparer.OrdinalIgnoreCase).Count();
        score += sharedTags * 15;

        // Keyword similarity
        var sharedKeywords = sourceKeywords.Intersect(targetKeywords).Count();
        var totalKeywords = sourceKeywords.Union(targetKeywords).Count();
        
        if (totalKeywords > 0)
        {
            var keywordSimilarity = (double)sharedKeywords / totalKeywords;
            score += (int)(keywordSimilarity * 100);
        }

        return score;
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

