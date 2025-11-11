# Wigo4it Coding Guidelines MCP Server

A Model Context Protocol (MCP) server built with .NET 9 that provides coding standards, style guides, Architecture Decision Records (ADRs), and recommendations from markdown documentation.

## Features

- **Document-Based**: Load documentation from markdown files
- **Dual Source Support**: Automatically detects environment and loads from local filesystem or GitHub
- **Rich Querying**: Search by type, category, language, or keywords
- **MCP Integration**: Seamless integration with GitHub Copilot and other MCP clients
- **Extensible**: Easy to add new documents by simply creating markdown files

## Available Tools

The MCP server exposes 7 tools for accessing documentation:

1. **GetAllDocuments**: List all available documents with summaries
2. **GetDocumentById**: Get a specific document by its ID
3. **GetDocumentByPath**: Get a document by its file path
4. **GetDocumentsByType**: Filter by type (CodingGuideline, StyleGuide, ADR, Recommendation)
5. **GetDocumentsByCategory**: Filter by category
6. **GetDocumentsByLanguage**: Filter by programming language
7. **SearchDocuments**: Search by keyword in title, content, or tags

## Quick Start

### Build and Run

```bash
cd src
dotnet build
cd Wigo4it.CodingGuidelines.McpServer
dotnet run
```

### Configure in VS Code

Add to `.vscode/mcp.json`:

```json
{
    "inputs": [],
    "servers": {
        "wigo4it-coding-guidelines": {
            "type": "stdio",
            "command": "dotnet",
            "args": [
                "run",
                "--project",
                "path/to/Wigo4it.CodingGuidelines.McpServer.csproj"
            ]
        }
    }
}
```

## Document Structure

```
docs/
├── guidelines/          # Coding guidelines
├── styles/             # Style guides
├── adr/                # Architecture Decision Records
└── recommendations/    # Best practice recommendations
```

Each document is a markdown file that's automatically indexed and made available through the MCP server.

## Example Usage

Ask GitHub Copilot:
- "Show me all coding guidelines"
- "What are the C# naming conventions?"  
- "Get the style guide for C#"
- "Show me all ADRs"
- "Search for dependency injection"

## How It Works

The server automatically detects its environment:
- **Local Development**: Loads from `docs/` folder in repository
- **Deployed**: Loads from GitHub repository via API

This means you can develop locally and deploy to production without code changes!

For more details, see the full documentation in the project.
