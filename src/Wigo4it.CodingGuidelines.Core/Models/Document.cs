namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a documentation document (coding guideline, style guide, ADR, or recommendation)
/// </summary>
public class Document
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Type { get; set; } // CodingGuideline, StyleGuide, ADR, Recommendation
    public required string Path { get; set; }
    public string? Category { get; set; }
    public string? Language { get; set; }
    public List<string> Tags { get; set; } = new();
    public DateTime? LastModified { get; set; }
}
