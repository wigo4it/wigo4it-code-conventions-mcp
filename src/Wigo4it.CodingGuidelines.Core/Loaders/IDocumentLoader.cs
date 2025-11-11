using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Loaders;

/// <summary>
/// Interface for loading documents from various sources
/// </summary>
public interface IDocumentLoader
{
    /// <summary>
    /// Loads all documents from the configured source
    /// </summary>
    /// <returns>List of loaded documents</returns>
    Task<List<Document>> LoadDocumentsAsync();
    
    /// <summary>
    /// Gets a document by its relative path
    /// </summary>
    /// <param name="path">The relative path to the document</param>
    /// <returns>The document or null if not found</returns>
    Task<Document?> GetDocumentByPathAsync(string path);
}
