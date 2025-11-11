# GitHub Copilot Instructions for Wigo4it Code Conventions MCP Server

## Project Purpose

This project serves as an **MCP (Model Context Protocol) Server** that provides documentation and coding guidelines for all Wigo4it software systems. It acts as a knowledge base accessible to AI assistants like GitHub Copilot.

### Important: This Project is Exception to Coding Standards

**This project itself does NOT need to comply with the coding standards and guidelines it serves.** Its sole purpose is to make documentation available through the MCP protocol. The guidelines and standards served by this MCP server apply to OTHER Wigo4it projects, not this one.

## Project Goals

1. **Serve Documentation**: Provide coding standards, style guides, ADRs, and recommendations as structured data
2. **Dual Source Support**: Load documents from GitHub (production) or local filesystem (development)
3. **Automatic Discovery**: Maintain awareness of all documents in the `docs/` folder
4. **MCP Integration**: Expose documents through 7 MCP tools for AI assistant consumption

## Solution Structure

### Three-Project Architecture

```
src/
├── Wigo4it.CodingGuidelines.Core/          # Core library
│   ├── Models/                              # Data models
│   │   ├── Document.cs                      # Unified document model
│   │   ├── DocumentSummary.cs               # Lightweight document info
│   │   └── DocumentType.cs                  # Enum for document categories
│   ├── Configuration/                       # Configuration models
│   │   └── DocumentSourceConfiguration.cs   # GitHub/local settings
│   ├── Loaders/                             # Document loading strategies
│   │   ├── IDocumentLoader.cs               # Loader interface
│   │   ├── LocalDocumentLoader.cs           # Filesystem implementation
│   │   └── GitHubDocumentLoader.cs          # GitHub API implementation
│   └── Services/                            # Business logic
│       └── DocumentService.cs               # Caching, filtering, search
│
├── Wigo4it.CodingGuidelines.McpServer/     # MCP Server host
│   ├── Tools/                               # MCP tool definitions
│   │   └── DocumentTools.cs                 # 7 MCP tools
│   └── Program.cs                           # Host with environment detection
│
└── Wigo4it.CodingGuidelines.Tests/         # Unit tests
    └── GuidelinesServiceTests.cs            # Service layer tests
```

### Document Organization

```
docs/
├── guidelines/        # Coding guidelines (naming, patterns, etc.)
├── styles/           # Style guides (formatting, structure)
├── adr/              # Architecture Decision Records
└── recommendations/  # Best practices and recommendations
```

## Key Configuration Principles

### Environment Detection (Program.cs)

The MCP server automatically detects its environment:

- **Local Development**: Detects `.git` or `.sln` files in parent directories → uses `LocalDocumentLoader`
- **Production/Deployed**: No repository markers found → uses `GitHubDocumentLoader`

```csharp
var projectRoot = FindProjectRoot();
var isLocal = projectRoot != null;

if (isLocal)
{
    // Load from filesystem: {projectRoot}/docs/
    services.AddSingleton<IDocumentLoader>(sp => 
        new LocalDocumentLoader(config, sp.GetRequiredService<ILogger<LocalDocumentLoader>>()));
}
else
{
    // Load from GitHub API
    services.AddSingleton<IDocumentLoader>(sp => 
        new GitHubDocumentLoader(config, httpClient, sp.GetRequiredService<ILogger<GitHubDocumentLoader>>()));
}
```

### Document Discovery and Indexing

**CRITICAL**: The MCP server MUST always be aware of all documents in the `docs/` folder.

#### How Document Indexing Works

1. **LocalDocumentLoader**: Recursively scans `docs/` directory on startup
   - Reads all `.md` files
   - Parses metadata from markdown frontmatter
   - Extracts document type from folder structure
   - Caches documents in memory

2. **GitHubDocumentLoader**: Queries GitHub Contents API recursively
   - Traverses `docs/` folder structure via API
   - Downloads file content for each markdown file
   - Parses same metadata as local loader
   - Caches documents in memory

3. **DocumentService**: Provides cached access with thread-safety
   - First call triggers document loading
   - Subsequent calls return cached results
   - Cache remains valid for application lifetime

#### Keeping Index Updated

When adding, modifying, or removing documents in `docs/`:

**Development (Local)**:
- Restart the MCP server to reload documents from filesystem
- Server automatically discovers new files on next startup

**Production (GitHub)**:
- Push changes to GitHub repository
- Server automatically fetches new content on next startup
- GitHub API provides fresh content

**No Manual Index Required**: The system automatically discovers all markdown files. There is NO separate index file to maintain.

### Document Format

All documents in `docs/` should follow this markdown structure:

```markdown
---
title: Document Title
category: CategoryName
language: ProgrammingLanguage
tags: [tag1, tag2, tag3]
---

# Document Title

Content goes here...
```

Metadata is optional but recommended for better searchability.

## MCP Tools Exposed

The server exposes 7 tools for document access:

1. `GetAllDocuments` - List all documents (returns summaries)
2. `GetDocumentById` - Get specific document by ID
3. `GetDocumentByPath` - Get document by file path
4. `GetDocumentsByType` - Filter by type (CodingGuideline, StyleGuide, ADR, Recommendation)
5. `GetDocumentsByCategory` - Filter by category (e.g., "naming", "async")
6. `GetDocumentsByLanguage` - Filter by programming language (e.g., "csharp")
7. `SearchDocuments` - Full-text search across all documents

## Development Guidelines for This Project

### When Adding New Features

1. **New Document Types**: Update `DocumentType` enum and parsing logic in loaders
2. **New MCP Tools**: Add to `DocumentTools.cs` with `[McpServerTool]` attribute
3. **New Loaders**: Implement `IDocumentLoader` interface
4. **Configuration Changes**: Update `DocumentSourceConfiguration.cs`

### When Adding Documentation

1. Create markdown file in appropriate `docs/` subfolder
2. Include metadata frontmatter (title, category, language, tags)
3. Use clear heading structure
4. Restart MCP server (local) or push to GitHub (production)
5. Verify with `GetAllDocuments` tool

### Testing Philosophy

- **Unit Tests**: Focus on DocumentService logic, filtering, caching
- **Integration Tests**: Not required - MCP SDK handles tool invocation
- **Manual Testing**: Use MCP Inspector or configure VS Code with `.vscode/mcp.json`

### Code Style for This Project

Since this project serves guidelines but doesn't follow them:

- **Prioritize Clarity**: Code should be easy to understand
- **Favor Simplicity**: Don't over-engineer
- **Document Intent**: Use comments for non-obvious logic
- **.NET Conventions**: Follow standard C# patterns (PascalCase, async/await, etc.)
- **No Strict Enforcement**: This project is exempt from the guidelines it serves

## Common Tasks

### Task: Add a New Document

1. Create markdown file: `docs/{subfolder}/{filename}.md`
2. Add frontmatter metadata
3. Write content with clear headings
4. Restart MCP server or push to GitHub
5. Test with `GetDocumentByPath` or `SearchDocuments`

### Task: Add a New MCP Tool

1. Open `DocumentTools.cs`
2. Add new method with `[McpServerTool(name, description)]` attribute
3. Inject `DocumentService` via constructor
4. Implement tool logic using DocumentService methods
5. Rebuild solution
6. Restart MCP server

### Task: Change Document Source Configuration

1. Open `Program.cs`
2. Update `DocumentSourceConfiguration` initialization
3. Modify GitHub repository, owner, or branch
4. Modify local docs folder path
5. Rebuild and restart

### Task: Debug Environment Detection

1. Run with debugger attached
2. Set breakpoint in `FindProjectRoot()` method
3. Verify detection logic finds `.git` or `.sln`
4. Check which `IDocumentLoader` is registered
5. Verify documents are loaded correctly

## Integration with GitHub Copilot

### VS Code Configuration

Create `.vscode/mcp.json` (in project using the guidelines, not this project):

```json
{
  "mcpServers": {
    "wigo4it-guidelines": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\Projects\\github.com\\Wigo4it\\wigo4it-code-conventions-mcp\\src\\Wigo4it.CodingGuidelines.McpServer"
      ]
    }
  }
}
```

### Example Copilot Queries

When configured, users can ask:

- "What are the C# naming conventions?"
- "Show me all coding guidelines"
- "What are the recommendations for dependency injection?"
- "Find documents about async programming"

Copilot will invoke the appropriate MCP tools to retrieve documentation.

## Deployment Considerations

### Local Development

- Runs from source with `dotnet run`
- Loads documents from filesystem
- Fast iteration on documentation changes
- No API rate limits

### Production Deployment

- Published as self-contained executable
- Loads documents from GitHub API
- Rate limited to 60 requests/hour (unauthenticated)
- Add GitHub token for 5000 requests/hour

## Summary

This MCP server is a **documentation delivery system**, not a typical application project. When working on it:

1. **Remember**: It serves guidelines but doesn't follow them
2. **Focus**: Keep document index automatically synchronized
3. **Maintain**: Ensure both loaders (local/GitHub) stay in sync
4. **Test**: Verify documents are discoverable after changes
5. **Document**: Update this file when architecture changes

The goal is reliable, automatic document discovery and delivery through MCP tools. Everything else is secondary.
