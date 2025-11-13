namespace Wigo4it.CodeGuidelines.Server.Models;

/// <summary>
/// Represents the full content of a documentation file.
/// </summary>
public sealed class DocumentationContent
{
    /// <summary>
    /// Gets or sets the metadata for the documentation.
    /// </summary>
    public required DocumentationMetadata Metadata { get; init; }

    /// <summary>
    /// Gets or sets the markdown content of the documentation.
    /// </summary>
    public required string Content { get; init; }
}
