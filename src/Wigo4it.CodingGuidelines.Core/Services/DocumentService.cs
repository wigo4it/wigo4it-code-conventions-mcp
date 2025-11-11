using Wigo4it.CodingGuidelines.Core.Loaders;
using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Services;

/// <summary>
/// Service for managing and accessing documents
/// </summary>
public class DocumentService
{
    private readonly IDocumentLoader _documentLoader;
    private List<Document>? _cachedDocuments;
    private readonly SemaphoreSlim _cacheLock = new(1, 1);

    public DocumentService(IDocumentLoader documentLoader)
    {
        _documentLoader = documentLoader;
    }

    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        await EnsureDocumentsLoadedAsync();
        return _cachedDocuments ?? new List<Document>();
    }

    public async Task<List<DocumentSummary>> GetAllDocumentSummariesAsync()
    {
        var documents = await GetAllDocumentsAsync();
        return documents.Select(d => new DocumentSummary
        {
            Id = d.Id,
            Title = d.Title,
            Type = d.Type,
            Path = d.Path,
            Category = d.Category,
            Language = d.Language,
            Tags = d.Tags
        }).ToList();
    }

    public async Task<Document?> GetDocumentByIdAsync(string id)
    {
        var documents = await GetAllDocumentsAsync();
        return documents.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<Document?> GetDocumentByPathAsync(string path)
    {
        var documents = await GetAllDocumentsAsync();
        return documents.FirstOrDefault(d => d.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<Document>> GetDocumentsByTypeAsync(string type)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<Document>> GetDocumentsByCategoryAsync(string category)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Category != null && d.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<Document>> GetDocumentsByLanguageAsync(string language)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Language != null && d.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    public async Task<List<Document>> SearchDocumentsAsync(string searchTerm)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       d.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       d.Tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    public async Task RefreshDocumentsAsync()
    {
        await _cacheLock.WaitAsync();
        try
        {
            _cachedDocuments = null;
            await EnsureDocumentsLoadedAsync();
        }
        finally
        {
            _cacheLock.Release();
        }
    }

    private async Task EnsureDocumentsLoadedAsync()
    {
        if (_cachedDocuments != null)
        {
            return;
        }

        await _cacheLock.WaitAsync();
        try
        {
            if (_cachedDocuments == null)
            {
                _cachedDocuments = await _documentLoader.LoadDocumentsAsync();
            }
        }
        finally
        {
            _cacheLock.Release();
        }
    }
}
