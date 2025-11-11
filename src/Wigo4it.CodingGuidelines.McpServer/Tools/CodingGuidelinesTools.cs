using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Services;

namespace Wigo4it.CodingGuidelines.McpServer.Tools;

[McpServerToolType]
public static class DocumentTools
{
    [McpServerTool, Description("Get a list of all available documents including their summaries.")]
    public static async Task<string> GetAllDocuments(DocumentService documentService)
    {
        var summaries = await documentService.GetAllDocumentSummariesAsync();
        return JsonSerializer.Serialize(summaries, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get a specific document by its ID.")]
    public static async Task<string> GetDocumentById(
        DocumentService documentService,
        [Description("The ID of the document to retrieve")] string id)
    {
        var document = await documentService.GetDocumentByIdAsync(id);
        if (document == null)
        {
            return $"Document with ID '{id}' not found.";
        }
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get a specific document by its path (e.g., 'guidelines/csharp-naming.md').")]
    public static async Task<string> GetDocumentByPath(
        DocumentService documentService,
        [Description("The path of the document to retrieve")] string path)
    {
        var document = await documentService.GetDocumentByPathAsync(path);
        if (document == null)
        {
            return $"Document at path '{path}' not found.";
        }
        return JsonSerializer.Serialize(document, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get documents by type (CodingGuideline, StyleGuide, ADR, Recommendation).")]
    public static async Task<string> GetDocumentsByType(
        DocumentService documentService,
        [Description("The type to filter by (CodingGuideline, StyleGuide, ADR, Recommendation)")] string type)
    {
        var documents = await documentService.GetDocumentsByTypeAsync(type);
        if (documents.Count == 0)
        {
            return $"No documents found for type '{type}'.";
        }
        return JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get documents by category.")]
    public static async Task<string> GetDocumentsByCategory(
        DocumentService documentService,
        [Description("The category to filter by")] string category)
    {
        var documents = await documentService.GetDocumentsByCategoryAsync(category);
        if (documents.Count == 0)
        {
            return $"No documents found for category '{category}'.";
        }
        return JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get documents for a specific programming language (e.g., C#, TypeScript).")]
    public static async Task<string> GetDocumentsByLanguage(
        DocumentService documentService,
        [Description("The programming language to filter by")] string language)
    {
        var documents = await documentService.GetDocumentsByLanguageAsync(language);
        if (documents.Count == 0)
        {
            return $"No documents found for language '{language}'.";
        }
        return JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Search for documents by keyword in title, content, or tags.")]
    public static async Task<string> SearchDocuments(
        DocumentService documentService,
        [Description("The search term to look for")] string searchTerm)
    {
        var documents = await documentService.SearchDocumentsAsync(searchTerm);
        if (documents.Count == 0)
        {
            return $"No documents found matching '{searchTerm}'.";
        }
        return JsonSerializer.Serialize(documents, new JsonSerializerOptions { WriteIndented = true });
    }
}
