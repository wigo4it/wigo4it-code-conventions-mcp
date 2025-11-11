using Wigo4it.CodingGuidelines.Core.Models;

namespace Wigo4it.CodingGuidelines.Core.Loaders;

/// <summary>
/// Interface for loading documents from various sources
/// </summary>
public interface IDocumentLoader
{
    Task<List<Document>> LoadDocumentsAsync();
    Task<Document?> GetDocumentByPathAsync(string path);
}
