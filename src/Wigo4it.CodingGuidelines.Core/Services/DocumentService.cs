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

    /// <summary>
    /// Initializes a new instance of the DocumentService class
    /// </summary>
    /// <param name="documentLoader">The document loader to use</param>
    public DocumentService(IDocumentLoader documentLoader)
    {
        _documentLoader = documentLoader;
    }

    /// <summary>
    /// Gets all documents
    /// </summary>
    /// <returns>List of all documents</returns>
    public async Task<List<Document>> GetAllDocumentsAsync()
    {
        await EnsureDocumentsLoadedAsync();
        return _cachedDocuments ?? new List<Document>();
    }

    /// <summary>
    /// Gets summaries of all documents
    /// </summary>
    /// <returns>List of document summaries</returns>
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

    /// <summary>
    /// Gets a document by its ID
    /// </summary>
    /// <param name="id">The document ID</param>
    /// <returns>The document or null if not found</returns>
    public async Task<Document?> GetDocumentByIdAsync(string id)
    {
        var documents = await GetAllDocumentsAsync();
        return documents.FirstOrDefault(d => d.Id.Equals(id, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets a document by its path
    /// </summary>
    /// <param name="path">The document path</param>
    /// <returns>The document or null if not found</returns>
    public async Task<Document?> GetDocumentByPathAsync(string path)
    {
        var documents = await GetAllDocumentsAsync();
        return documents.FirstOrDefault(d => d.Path.Equals(path, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets documents filtered by type
    /// </summary>
    /// <param name="type">The document type to filter by</param>
    /// <returns>List of documents of the specified type</returns>
    public async Task<List<Document>> GetDocumentsByTypeAsync(string type)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Type.Equals(type, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets documents filtered by category
    /// </summary>
    /// <param name="category">The category to filter by</param>
    /// <returns>List of documents in the specified category</returns>
    public async Task<List<Document>> GetDocumentsByCategoryAsync(string category)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Category != null && d.Category.Equals(category, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Gets documents for a specific programming language
    /// </summary>
    /// <param name="language">The programming language to filter by</param>
    /// <returns>List of documents for the specified language</returns>
    public async Task<List<Document>> GetDocumentsByLanguageAsync(string language)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Language != null && d.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    /// <summary>
    /// Searches for documents by keyword
    /// </summary>
    /// <param name="searchTerm">The search term</param>
    /// <returns>List of documents matching the search term</returns>
    public async Task<List<Document>> SearchDocumentsAsync(string searchTerm)
    {
        var documents = await GetAllDocumentsAsync();
        return documents
            .Where(d => d.Title.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       d.Content.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
                       d.Tags.Any(t => t.Contains(searchTerm, StringComparison.OrdinalIgnoreCase)))
            .ToList();
    }

    /// <summary>
    /// Refreshes the cached documents by reloading from the source
    /// </summary>
    /// <returns>A task representing the asynchronous operation</returns>
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
