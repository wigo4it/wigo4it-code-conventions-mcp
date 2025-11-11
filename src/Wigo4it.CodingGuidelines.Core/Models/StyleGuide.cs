namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a style guide
/// </summary>
public class StyleGuide
{
    /// <summary>
    /// Gets or sets the unique identifier
    /// </summary>
    public required string Id { get; set; }
    
    /// <summary>
    /// Gets or sets the title
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Gets or sets the content
    /// </summary>
    public required string Content { get; set; }
    
    /// <summary>
    /// Gets or sets the programming language
    /// </summary>
    public required string Language { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
