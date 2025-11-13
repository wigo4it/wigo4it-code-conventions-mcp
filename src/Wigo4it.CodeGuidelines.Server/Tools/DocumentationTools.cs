using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;

namespace Wigo4it.CodeGuidelines.Server.Tools;

/// <summary>
/// MCP tools for accessing code guidelines documentation.
/// </summary>
[McpServerToolType]
public static class DocumentationTools
{
    [McpServerTool]
    [Description("Gets a list of all available code guidelines documentation with metadata including title, category, and description.")]
    public static async Task<string> GetAllDocumentation(IDocumentationService documentationService)
    {
        var docs = await documentationService.GetAllDocumentationAsync();
        return JsonSerializer.Serialize(docs, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets all documentation for a specific category. Valid categories are: ADRs, Recommendations, StyleGuides, Structures.")]
    public static async Task<string> GetDocumentationByCategory(
        IDocumentationService documentationService,
        [Description("The documentation category (ADRs, Recommendations, StyleGuides, or Structures)")]
        string category)
    {
        if (!Enum.TryParse<DocumentationCategory>(category, true, out var categoryEnum))
        {
            return JsonSerializer.Serialize(new
            {
                error = $"Invalid category '{category}'. Valid categories are: {string.Join(", ", Enum.GetNames<DocumentationCategory>())}"
            });
        }

        var docs = await documentationService.GetDocumentationByCategoryAsync(categoryEnum);
        return JsonSerializer.Serialize(docs, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Gets the full markdown content of a specific documentation file by its ID. The ID format is 'category/filename' (e.g., 'adrs/adr-001-use-mcp-server').")]
    public static async Task<string> GetDocumentationContent(
        IDocumentationService documentationService,
        [Description("The unique identifier of the documentation in format 'category/filename'")]
        string id)
    {
        var content = await documentationService.GetDocumentationContentAsync(id.ToLowerInvariant());

        if (content == null)
        {
            return JsonSerializer.Serialize(new
            {
                error = $"Documentation with ID '{id}' not found"
            });
        }

        return JsonSerializer.Serialize(new
        {
            content.Metadata.Id,
            content.Metadata.Title,
            content.Metadata.Category,
            content.Metadata.Description,
            content.Content
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}
