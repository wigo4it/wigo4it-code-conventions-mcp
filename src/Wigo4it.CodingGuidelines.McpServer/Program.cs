using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ModelContextProtocol.Server;
using Wigo4it.CodingGuidelines.Core.Configuration;
using Wigo4it.CodingGuidelines.Core.Loaders;
using Wigo4it.CodingGuidelines.Core.Services;

var builder = Host.CreateApplicationBuilder(args);

// Configure logging to stderr
builder.Logging.AddConsole(consoleLogOptions =>
{
    // Configure all logs to go to stderr
    consoleLogOptions.LogToStandardErrorThreshold = LogLevel.Trace;
});

// Configure document source based on environment
var configuration = new DocumentSourceConfiguration();

// Check if running locally (look for .git folder or specific environment variable)
var currentDirectory = Directory.GetCurrentDirectory();
var projectRoot = FindProjectRoot(currentDirectory);

if (projectRoot != null)
{
    // Running locally
    configuration.SourceType = DocumentSourceType.Local;
    configuration.LocalBasePath = projectRoot;
    builder.Logging.AddConsole().Services.AddLogging(logging =>
    {
        logging.AddFilter("Wigo4it", LogLevel.Information);
    });
}
else
{
    // Running from published/deployed location - use GitHub
    configuration.SourceType = DocumentSourceType.GitHub;
}

// Register configuration
builder.Services.AddSingleton(configuration);

// Register HttpClient for GitHub access
builder.Services.AddHttpClient();

// Register document loader based on configuration
builder.Services.AddSingleton<IDocumentLoader>(sp =>
{
    if (configuration.SourceType == DocumentSourceType.Local)
    {
        return new LocalDocumentLoader(configuration);
    }
    else
    {
        var httpClientFactory = sp.GetRequiredService<IHttpClientFactory>();
        var httpClient = httpClientFactory.CreateClient();
        return new GitHubDocumentLoader(httpClient, configuration);
    }
});

// Register the DocumentService
builder.Services.AddSingleton<DocumentService>();

// Configure MCP Server
builder.Services
    .AddMcpServer()
    .WithStdioServerTransport()
    .WithToolsFromAssembly();

await builder.Build().RunAsync();

static string? FindProjectRoot(string currentPath)
{
    var directory = new DirectoryInfo(currentPath);
    
    while (directory != null)
    {
        // Look for .git folder or solution file
        if (Directory.Exists(Path.Combine(directory.FullName, ".git")) ||
            Directory.GetFiles(directory.FullName, "*.sln").Length > 0)
        {
            return directory.FullName;
        }
        
        directory = directory.Parent;
    }
    
    return null;
}
