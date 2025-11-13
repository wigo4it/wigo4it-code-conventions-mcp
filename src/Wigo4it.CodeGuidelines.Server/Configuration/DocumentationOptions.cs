namespace Wigo4it.CodeGuidelines.Server.Configuration;

/// <summary>
/// Configuration options for documentation sources.
/// </summary>
public sealed class DocumentationOptions
{
    /// <summary>
    /// Configuration section name.
    /// </summary>
    public const string SectionName = "Documentation";

    /// <summary>
    /// Gets or sets the base path for documentation files.
    /// If null or empty, defaults to the GitHub repository docs folder.
    /// </summary>
    public string? BasePath { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to use local file system for debugging.
    /// Default is false (uses GitHub repository).
    /// </summary>
    public bool UseLocalFileSystem { get; set; }

    /// <summary>
    /// Gets or sets the GitHub repository owner.
    /// Default is "wigo4it".
    /// </summary>
    public string GitHubOwner { get; set; } = "wigo4it";

    /// <summary>
    /// Gets or sets the GitHub repository name.
    /// Default is "wigo4it-code-conventions-mcp".
    /// </summary>
    public string GitHubRepository { get; set; } = "wigo4it-code-conventions-mcp";

    /// <summary>
    /// Gets or sets the GitHub branch to use.
    /// Default is "main".
    /// </summary>
    public string GitHubBranch { get; set; } = "main";

    /// <summary>
    /// Gets or sets the documentation folder path within the repository.
    /// Default is "docs".
    /// </summary>
    public string DocsPath { get; set; } = "docs";

    /// <summary>
    /// Gets the effective base path, accounting for defaults.
    /// </summary>
    public string GetEffectiveBasePath()
    {
        if (!string.IsNullOrWhiteSpace(BasePath))
        {
            return BasePath;
        }

        // Default to docs folder relative to the repository root
        var repoRoot = FindRepositoryRoot();
        return Path.Combine(repoRoot, "docs");
    }

    private static string FindRepositoryRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        while (currentDir != null)
        {
            if (Directory.Exists(Path.Combine(currentDir, ".git")) ||
                Directory.Exists(Path.Combine(currentDir, "docs")))
            {
                return currentDir;
            }
            currentDir = Directory.GetParent(currentDir)?.FullName;
        }

        // Fallback to current directory
        return Directory.GetCurrentDirectory();
    }
}
