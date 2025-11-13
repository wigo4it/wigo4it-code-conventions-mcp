namespace Wigo4it.CodeGuidelines.Server.Models;

/// <summary>
/// Represents a search result with relevance information.
/// </summary>
public sealed class DocumentationSearchResult
{
    /// <summary>
    /// Gets or sets the document metadata.
    /// </summary>
    public required DocumentationMetadata Metadata { get; set; }

    /// <summary>
    /// Gets or sets the relevance score (0-100).
    /// </summary>
    public int RelevanceScore { get; set; }

    /// <summary>
    /// Gets or sets matching excerpts from the document.
    /// </summary>
    public List<string> MatchingExcerpts { get; set; } = [];

    /// <summary>
    /// Gets or sets the number of matches found in the document.
    /// </summary>
    public int MatchCount { get; set; }
}
