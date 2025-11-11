using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Services;

namespace Wigo4it.CodingGuidelines.McpServer.Tools;

[McpServerToolType]
public static class StyleGuideTools
{
    [McpServerTool, Description("Get all style guides available in the system.")]
    public static string GetAllStyleGuides(GuidelinesService guidelinesService)
    {
        var styleGuides = guidelinesService.GetAllStyleGuides();
        return JsonSerializer.Serialize(styleGuides, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get a specific style guide by its ID (e.g., SG001).")]
    public static string GetStyleGuideById(
        GuidelinesService guidelinesService,
        [Description("The ID of the style guide to retrieve")] string id)
    {
        var styleGuide = guidelinesService.GetStyleGuideById(id);
        if (styleGuide == null)
        {
            return $"Style guide with ID '{id}' not found.";
        }
        return JsonSerializer.Serialize(styleGuide, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get the style guide for a specific programming language (e.g., C#, TypeScript).")]
    public static string GetStyleGuideByLanguage(
        GuidelinesService guidelinesService,
        [Description("The programming language to get the style guide for")] string language)
    {
        var styleGuide = guidelinesService.GetStyleGuideByLanguage(language);
        if (styleGuide == null)
        {
            return $"Style guide for language '{language}' not found.";
        }
        return JsonSerializer.Serialize(styleGuide, new JsonSerializerOptions { WriteIndented = true });
    }
}
