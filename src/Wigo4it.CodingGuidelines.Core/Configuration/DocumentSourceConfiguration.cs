namespace Wigo4it.CodingGuidelines.Core.Configuration;

/// <summary>
/// Configuration for document sources
/// </summary>
public class DocumentSourceConfiguration
{
    public DocumentSourceType SourceType { get; set; } = DocumentSourceType.GitHub;
    public string? LocalBasePath { get; set; }
    public string GitHubOwner { get; set; } = "wigo4it";
    public string GitHubRepo { get; set; } = "wigo4it-code-conventions-mcp";
    public string GitHubBranch { get; set; } = "main";
    public string DocsPath { get; set; } = "docs";
}

public enum DocumentSourceType
{
    Local,
    GitHub
}
