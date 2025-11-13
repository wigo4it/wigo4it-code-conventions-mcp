using Wigo4it.CodeGuidelines.Server.Models;

namespace Wigo4it.CodeGuidelines.Server.Services;

/// <summary>
/// Service for managing and retrieving documentation.
/// </summary>
public interface IDocumentationService
{
    /// <summary>
    /// Gets all available documentation metadata.
    /// </summary>
    /// <returns>A collection of all documentation metadata.</returns>
    Task<IReadOnlyList<DocumentationMetadata>> GetAllDocumentationAsync();

    /// <summary>
    /// Gets documentation metadata filtered by category.
    /// </summary>
    /// <param name="category">The category to filter by.</param>
    /// <returns>A collection of documentation metadata for the specified category.</returns>
    Task<IReadOnlyList<DocumentationMetadata>> GetDocumentationByCategoryAsync(DocumentationCategory category);

    /// <summary>
    /// Gets the full content of a specific documentation file.
    /// </summary>
    /// <param name="id">The unique identifier of the documentation.</param>
    /// <returns>The documentation content, or null if not found.</returns>
    Task<DocumentationContent?> GetDocumentationContentAsync(string id);

    /// <summary>
    /// Searches across all documentation content using a search term.
    /// </summary>
    /// <param name="searchTerm">The term to search for.</param>
    /// <returns>A collection of matching documentation with relevance scores.</returns>
    Task<IReadOnlyList<DocumentationSearchResult>> SearchDocumentationAsync(string searchTerm);

    /// <summary>
    /// Gets documents related to a specific document based on content similarity.
    /// </summary>
    /// <param name="documentId">The document ID to find related documents for.</param>
    /// <param name="maxResults">Maximum number of related documents to return.</param>
    /// <returns>A collection of related documentation metadata.</returns>
    Task<IReadOnlyList<DocumentationMetadata>> GetRelatedDocumentationAsync(string documentId, int maxResults = 5);

    /// <summary>
    /// Gets documentation filtered by tags.
    /// </summary>
    /// <param name="tags">The tags to filter by.</param>
    /// <returns>A collection of documentation matching the specified tags.</returns>
    Task<IReadOnlyList<DocumentationMetadata>> GetDocumentationByTagsAsync(IEnumerable<string> tags);
}
