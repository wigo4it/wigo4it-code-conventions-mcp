namespace Wigo4it.CodingGuidelines.Core.Models;

/// <summary>
/// Represents an Architecture Decision Record (ADR)
/// </summary>
public class ArchitectureDecisionRecord
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
    /// Gets or sets the context
    /// </summary>
    public required string Context { get; set; }
    
    /// <summary>
    /// Gets or sets the decision
    /// </summary>
    public required string Decision { get; set; }
    
    /// <summary>
    /// Gets or sets the consequences
    /// </summary>
    public required string Consequences { get; set; }
    
    /// <summary>
    /// Gets or sets the status (Proposed, Accepted, Deprecated, Superseded)
    /// </summary>
    public required string Status { get; set; }
    
    /// <summary>
    /// Gets or sets the date created
    /// </summary>
    public DateTime DateCreated { get; set; }
    
    /// <summary>
    /// Gets or sets the list of tags
    /// </summary>
    public List<string> Tags { get; set; } = new();
}
