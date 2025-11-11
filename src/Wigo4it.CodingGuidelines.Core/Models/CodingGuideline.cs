namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a coding guideline or standard
/// </summary>
public class CodingGuideline
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
    /// Gets or sets the description
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Gets or sets the category
    /// </summary>
    public required string Category { get; set; }
    
    /// <summary>
    /// Gets or sets an optional code example
    /// </summary>
    public string? Example { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
    
    /// <summary>
    /// Gets or sets the programming language
    /// </summary>
    public string? Language { get; set; }
}
