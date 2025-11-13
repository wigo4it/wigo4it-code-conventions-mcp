namespace Wigo4it.CodeGuidelines.Server.Models;

/// <summary>
/// Represents metadata for a documentation file.
/// </summary>
public sealed class DocumentationMetadata
{
    /// <summary>
    /// Gets or sets the unique identifier for the documentation.
    /// </summary>
    public required string Id { get; init; }

    /// <summary>
    /// Gets or sets the title of the documentation.
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Gets or sets the category of the documentation.
    /// </summary>
    public required DocumentationCategory Category { get; init; }

    /// <summary>
    /// Gets or sets the file path of the documentation.
    /// </summary>
    public required string FilePath { get; init; }

    /// <summary>
    /// Gets or sets an optional description of the documentation.
    /// </summary>
    public string? Description { get; init; }

    /// <summary>
    /// Gets or sets the tags associated with the documentation.
    /// </summary>
    public List<string> Tags { get; init; } = [];
}
