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

    [McpServerTool]
    [Description("Searches all documentation for a given search term. Returns ranked results with relevance scores, matching excerpts, and match counts.")]
    public static async Task<string> SearchDocumentation(
        IDocumentationService documentationService,
        [Description("The search term to look for across all documentation")]
        string searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return JsonSerializer.Serialize(new
            {
                error = "Search term cannot be empty"
            });
        }

        var results = await documentationService.SearchDocumentationAsync(searchTerm);
        
        return JsonSerializer.Serialize(new
        {
            searchTerm,
            resultCount = results.Count,
            results = results.Select(r => new
            {
                r.Metadata.Id,
                r.Metadata.Title,
                r.Metadata.Category,
                r.Metadata.Description,
                r.RelevanceScore,
                r.MatchCount,
                r.MatchingExcerpts
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Finds documentation related to a specific document based on content similarity, shared tags, and category. Returns up to maxResults related documents.")]
    public static async Task<string> GetRelatedDocumentation(
        IDocumentationService documentationService,
        [Description("The unique identifier of the source documentation in format 'category/filename'")]
        string documentId,
        [Description("Maximum number of related documents to return (default: 5)")]
        int maxResults = 5)
    {
        if (string.IsNullOrWhiteSpace(documentId))
        {
            return JsonSerializer.Serialize(new
            {
                error = "Document ID cannot be empty"
            });
        }

        if (maxResults < 1 || maxResults > 20)
        {
            return JsonSerializer.Serialize(new
            {
                error = "maxResults must be between 1 and 20"
            });
        }

        var relatedDocs = await documentationService.GetRelatedDocumentationAsync(documentId.ToLowerInvariant(), maxResults);
        
        return JsonSerializer.Serialize(new
        {
            sourceDocumentId = documentId.ToLowerInvariant(),
            resultCount = relatedDocs.Count,
            relatedDocuments = relatedDocs.Select(d => new
            {
                d.Id,
                d.Title,
                d.Category,
                d.Description,
                d.Tags
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool]
    [Description("Filters documentation by one or more tags. Returns all documents that have at least one of the specified tags.")]
    public static async Task<string> GetDocumentationByTag(
        IDocumentationService documentationService,
        [Description("Comma-separated list of tags to filter by (e.g., 'architecture,aspire')")]
        string tags)
    {
        if (string.IsNullOrWhiteSpace(tags))
        {
            return JsonSerializer.Serialize(new
            {
                error = "Tags parameter cannot be empty"
            });
        }

        var tagList = tags.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        
        if (tagList.Length == 0)
        {
            return JsonSerializer.Serialize(new
            {
                error = "At least one tag must be specified"
            });
        }

        var docs = await documentationService.GetDocumentationByTagsAsync(tagList);
        
        return JsonSerializer.Serialize(new
        {
            searchTags = tagList,
            resultCount = docs.Count,
            documents = docs.Select(d => new
            {
                d.Id,
                d.Title,
                d.Category,
                d.Description,
                d.Tags
            })
        }, new JsonSerializerOptions { WriteIndented = true });
    }
}

