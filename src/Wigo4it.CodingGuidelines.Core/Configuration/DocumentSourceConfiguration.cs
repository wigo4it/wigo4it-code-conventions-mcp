namespace Wigo4it.CodingGuidelines.Core.Configuration;

/// <summary>
/// Configuration for document sources
/// </summary>
public class DocumentSourceConfiguration
{
    /// <summary>
    /// Gets or sets the source type (Local or GitHub)
    /// </summary>
    public DocumentSourceType SourceType { get; set; } = DocumentSourceType.GitHub;
    
    /// <summary>
    /// Gets or sets the local base path when using Local source type
    /// </summary>
    public string? LocalBasePath { get; set; }
    
    /// <summary>
    /// Gets or sets the GitHub repository owner
    /// </summary>
    public string GitHubOwner { get; set; } = "wigo4it";
    
    /// <summary>
    /// Gets or sets the GitHub repository name
    /// </summary>
    public string GitHubRepo { get; set; } = "wigo4it-code-conventions-mcp";
    
    /// <summary>
    /// Gets or sets the GitHub branch name
    /// </summary>
    public string GitHubBranch { get; set; } = "main";
    
    /// <summary>
    /// Gets or sets the path to the documentation folder
    /// </summary>
    public string DocsPath { get; set; } = "docs";
}

/// <summary>
/// Types of document sources
/// </summary>
public enum DocumentSourceType
{
    /// <summary>
    /// Load documents from local filesystem
    /// </summary>
    Local,
    
    /// <summary>
    /// Load documents from GitHub repository
    /// </summary>
    GitHub
}
