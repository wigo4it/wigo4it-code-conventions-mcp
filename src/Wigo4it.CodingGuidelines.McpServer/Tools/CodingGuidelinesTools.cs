using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Services;

namespace Wigo4it.CodingGuidelines.McpServer.Tools;

[McpServerToolType]
public static class CodingGuidelinesTools
{
    [McpServerTool, Description("Get all coding guidelines available in the system.")]
    public static string GetAllCodingGuidelines(GuidelinesService guidelinesService)
    {
        var guidelines = guidelinesService.GetAllCodingGuidelines();
        return JsonSerializer.Serialize(guidelines, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get a specific coding guideline by its ID (e.g., CG001).")]
    public static string GetCodingGuidelineById(
        GuidelinesService guidelinesService,
        [Description("The ID of the coding guideline to retrieve")] string id)
    {
        var guideline = guidelinesService.GetCodingGuidelineById(id);
        if (guideline == null)
        {
            return $"Coding guideline with ID '{id}' not found.";
        }
        return JsonSerializer.Serialize(guideline, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get coding guidelines by category (e.g., Naming, Design).")]
    public static string GetCodingGuidelinesByCategory(
        GuidelinesService guidelinesService,
        [Description("The category to filter by (e.g., Naming, Design)")] string category)
    {
        var guidelines = guidelinesService.GetCodingGuidelinesByCategory(category);
        if (guidelines.Count == 0)
        {
            return $"No coding guidelines found for category '{category}'.";
        }
        return JsonSerializer.Serialize(guidelines, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get coding guidelines for a specific programming language (e.g., C#, TypeScript).")]
    public static string GetCodingGuidelinesByLanguage(
        GuidelinesService guidelinesService,
        [Description("The programming language to filter by")] string language)
    {
        var guidelines = guidelinesService.GetCodingGuidelinesByLanguage(language);
        if (guidelines.Count == 0)
        {
            return $"No coding guidelines found for language '{language}'.";
        }
        return JsonSerializer.Serialize(guidelines, new JsonSerializerOptions { WriteIndented = true });
    }
}
