using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Services;

namespace Wigo4it.CodingGuidelines.McpServer.Tools;

[McpServerToolType]
public static class ADRTools
{
    [McpServerTool, Description("Get all Architecture Decision Records (ADRs) available in the system.")]
    public static string GetAllADRs(GuidelinesService guidelinesService)
    {
        var adrs = guidelinesService.GetAllADRs();
        return JsonSerializer.Serialize(adrs, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get a specific ADR by its ID (e.g., ADR001).")]
    public static string GetADRById(
        GuidelinesService guidelinesService,
        [Description("The ID of the ADR to retrieve")] string id)
    {
        var adr = guidelinesService.GetADRById(id);
        if (adr == null)
        {
            return $"ADR with ID '{id}' not found.";
        }
        return JsonSerializer.Serialize(adr, new JsonSerializerOptions { WriteIndented = true });
    }

    [McpServerTool, Description("Get ADRs by status (e.g., Proposed, Accepted, Deprecated, Superseded).")]
    public static string GetADRsByStatus(
        GuidelinesService guidelinesService,
        [Description("The status to filter by (Proposed, Accepted, Deprecated, Superseded)")] string status)
    {
        var adrs = guidelinesService.GetADRsByStatus(status);
        if (adrs.Count == 0)
        {
            return $"No ADRs found with status '{status}'.";
        }
        return JsonSerializer.Serialize(adrs, new JsonSerializerOptions { WriteIndented = true });
    }
}
