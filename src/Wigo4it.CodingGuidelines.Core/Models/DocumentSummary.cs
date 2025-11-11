namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Summary information about a document
/// </summary>
public class DocumentSummary
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
    /// Gets or sets the type of document
    /// </summary>
    public required string Type { get; set; }
    
    /// <summary>
    /// Gets or sets the relative path to the document
    /// </summary>
    public required string Path { get; set; }
    
    /// <summary>
    /// Gets or sets the category of the document
    /// </summary>
    public string? Category { get; set; }
    
    /// <summary>
    /// Gets or sets the programming language the document applies to
    /// </summary>
    public string? Language { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags associated with the document
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
