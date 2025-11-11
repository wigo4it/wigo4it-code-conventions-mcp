namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Summary information about a document
/// </summary>
public class DocumentSummary
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Type { get; set; }
    public required string Path { get; set; }
    public string? Category { get; set; }
    public string? Language { get; set; }
    public List<string> Tags { get; set; } = new();
}
