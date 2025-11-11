namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents a coding guideline or standard
/// </summary>
public class CodingGuideline
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Description { get; set; }
    public required string Category { get; set; }
    public string? Example { get; set; }
    public List<string> Tags { get; set; } = new();
    public string? Language { get; set; }
}
