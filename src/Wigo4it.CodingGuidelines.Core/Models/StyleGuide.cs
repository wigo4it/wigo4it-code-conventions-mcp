namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a style guide
/// </summary>
public class StyleGuide
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Content { get; set; }
    public required string Language { get; set; }
    public List<string> Tags { get; set; } = new();
}
