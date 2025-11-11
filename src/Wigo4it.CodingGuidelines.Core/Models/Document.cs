namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a documentation document (coding guideline, style guide, ADR, or recommendation)
/// </summary>
public class Document
{
    /// <summary>
    /// Gets or sets the unique identifier for the document
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the title of the document
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the full content of the document in markdown format
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// Gets or sets the type of document (CodingGuideline, StyleGuide, ADR, Recommendation)
    /// </summary>
    public required string Type { get; set; }
    
    /// <summary>
    /// Gets or sets the relative path to the document within the docs folder
    /// </summary>
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the category of the document (e.g., naming, async, architecture)
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Gets or sets the programming language the document applies to (e.g., C#, TypeScript)
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags associated with the document
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the last modification date of the document
    /// </summary>
    public DateTime? LastModified { get; set; }
}
