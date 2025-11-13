namespace Wigo4it.CodeGuidelines.Server.Models;

/// <summary>
/// Represents the categories of documentation available in the system.
/// </summary>
public enum DocumentationCategory
{
    /// <summary>
    /// Architecture Decision Records.
    /// </summary>
    ADRs,

    /// <summary>
    /// Best practices and guidelines.
    /// </summary>
    Recommendations,

    /// <summary>
    /// Coding style and formatting standards.
    /// </summary>
    StyleGuides,

    /// <summary>
    /// Project and code organization patterns.
    /// </summary>
    Structures
}
