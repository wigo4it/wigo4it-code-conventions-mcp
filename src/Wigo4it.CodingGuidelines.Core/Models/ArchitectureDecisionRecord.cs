namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents an Architecture Decision Record (ADR)
/// </summary>
public class ArchitectureDecisionRecord
{
    public required string Id { get; set; }
    public required string Title { get; set; }
    public required string Context { get; set; }
    public required string Decision { get; set; }
    public required string Consequences { get; set; }
    public required string Status { get; set; } // Proposed, Accepted, Deprecated, Superseded
    public DateTime DateCreated { get; set; }
    public List<string> Tags { get; set; } = new();
}
