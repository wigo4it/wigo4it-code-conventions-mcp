using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Concurrent;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;

namespace Wigo4it.CodeGuidelines.Server.Services;

/// <summary>
/// Local file system implementation of the documentation service.
/// </summary>
public sealed class LocalFileSystemDocumentationService : IDocumentationService
{
    private readonly DocumentationOptions _options;
    private readonly ILogger<LocalFileSystemDocumentationService> _logger;
    private readonly ConcurrentDictionary<string, DocumentationMetadata> _cache = new();
    private bool _isInitialized;
    private readonly SemaphoreSlim _initLock = new(1, 1);

    public LocalFileSystemDocumentationService(
        IOptions<DocumentationOptions> options,
        ILogger<LocalFileSystemDocumentationService> logger)
    {
        _options = options.Value;
        _logger = logger;
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

        try
        {
            var content = await File.ReadAllTextAsync(metadata.FilePath);
            return new DocumentationContent
            {
                Metadata = metadata,
                Content = content
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to read documentation file: {FilePath}", metadata.FilePath);
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
        var basePath = _options.GetEffectiveBasePath();
        _logger.LogInformation("Initializing documentation cache from: {BasePath}", basePath);

        if (!Directory.Exists(basePath))
        {
            _logger.LogWarning("Documentation base path does not exist: {BasePath}", basePath);
            return;
        }

        // Scan each category folder
        foreach (var category in Enum.GetValues<DocumentationCategory>())
        {
            var categoryPath = Path.Combine(basePath, category.ToString());
            if (!Directory.Exists(categoryPath))
            {
                _logger.LogDebug("Category folder not found: {CategoryPath}", categoryPath);
                continue;
            }

            await ScanCategoryFolderAsync(categoryPath, category);
        }

        _logger.LogInformation("Initialized {Count} documentation files", _cache.Count);
    }

    private async Task ScanCategoryFolderAsync(string categoryPath, DocumentationCategory category)
    {
        var markdownFiles = Directory.GetFiles(categoryPath, "*.md", SearchOption.AllDirectories);

        foreach (var filePath in markdownFiles)
        {
            try
            {
                var metadata = await CreateMetadataFromFileAsync(filePath, category);
                _cache.TryAdd(metadata.Id, metadata);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process documentation file: {FilePath}", filePath);
            }
        }
    }

    private async Task<DocumentationMetadata> CreateMetadataFromFileAsync(string filePath, DocumentationCategory category)
    {
        var fileName = Path.GetFileNameWithoutExtension(filePath);
        var id = $"{category}/{fileName}".ToLowerInvariant();

        // Read first few lines to extract title
        var title = fileName;
        var description = string.Empty;

        try
        {
            var lines = await File.ReadAllLinesAsync(filePath);
            if (lines.Length > 0)
            {
                // Look for markdown title (# Title)
                var titleLine = lines.FirstOrDefault(l => l.TrimStart().StartsWith("# "));
                if (titleLine != null)
                {
                    title = titleLine.TrimStart('#').Trim();
                }

                // Look for description in first paragraph
                var descLines = lines.Skip(1).TakeWhile(l => !string.IsNullOrWhiteSpace(l)).Take(3);
                description = string.Join(" ", descLines).Trim();
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to extract title/description from: {FilePath}", filePath);
        }

        return new DocumentationMetadata
        {
            Id = id,
            Title = title,
            Category = category,
            FilePath = filePath,
            Description = string.IsNullOrWhiteSpace(description) ? null : description
        };
    }
}
