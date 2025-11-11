using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Register the GuidelinesService
builder.Services.AddSingleton<GuidelinesService>();

// Configure MCP Server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();
