using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr (required for MCP stdio transport)
builder.Logging.AddConsole(consoleLogOptions =>
{
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configure documentation options
builder.Services.Configure<DocumentationOptions>(
    builder.Configuration.GetSection(DocumentationOptions.SectionName));

// Register services
builder.Services.AddSingleton<IDocumentationService, DocumentationService>();

// Configure MCP server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
