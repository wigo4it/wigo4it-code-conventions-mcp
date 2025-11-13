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
}
