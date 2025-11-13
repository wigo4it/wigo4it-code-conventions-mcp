using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Services;
using Wigo4it.CodeGuidelines.Server.Tools;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (required for MCP stdio transport)
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configure documentation options
builder.Services.Configure<DocumentationOptions>(
    builder.Configuration.GetSection(DocumentationOptions.SectionName));

// Register HttpClient for GitHub API
builder.Services.AddHttpClient("GitHub");

// Register appropriate documentation service based on configuration
var useLocalFileSystem = builder.Configuration
    .GetSection(DocumentationOptions.SectionName)
    .GetValue<bool>(nameof(DocumentationOptions.UseLocalFileSystem));

if (useLocalFileSystem)
{
    builder.Services.AddSingleton<IDocumentationService, LocalFileSystemDocumentationService>();
}
else
{
    builder.Services.AddSingleton<IDocumentationService, GitHubDocumentationService>();
}

// Configure MCP server - explicitly specify the assembly containing the tools
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly(typeof(DocumentationTools).Assembly);

await builder.Build().RunAsync();
