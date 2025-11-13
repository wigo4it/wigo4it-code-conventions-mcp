# GitHub Copilot Instructions

## Project Overview

This is an MCP (Model Context Protocol) server project that provides access to technical documentation and Architecture Decision Records (ADRs). The server exposes documentation resources to MCP clients.

**Repository**: https://github.com/wigo4it/wigo4it-code-conventions-mcp (Private)

## Technology Stack

- **.NET Version**: .NET 10
- **C# Version**: C# 14
- **Project Type**: MCP Server
- **Architecture**: Model Context Protocol implementation

## Project Structure

### Source Code
- All .NET projects must be created in the `src/` folder
- Follow standard .NET solution and project organization
- **Solution**: `src/wigo4it-code-conventions-mcp.sln`

### Projects

#### Wigo4it.CodeGuidelines.McpServer
- **Type**: Console Application (.NET 10)
- **Purpose**: MCP server host that exposes documentation as MCP tools
- **Location**: `src/Wigo4it.CodeGuidelines.McpServer/`
- **Key Features**:
  - Uses stdio transport for MCP communication
  - Configures logging to stderr
  - Hosts the MCP server with Microsoft.Extensions.Hosting

#### Wigo4it.CodeGuidelines.Server
- **Type**: Class Library (.NET 10)
- **Purpose**: Core server implementation containing services and MCP tools
- **Location**: `src/Wigo4it.CodeGuidelines.Server/`
- **Key Components**:
  - **Models**: DocumentationMetadata, DocumentationContent, DocumentationCategory
  - **Services**: IDocumentationService, DocumentationService
  - **Tools**: DocumentationTools (MCP tool implementations)
  - **Configuration**: DocumentationOptions
- **Key Features**:
  - File-based documentation scanning and caching
  - Support for 4 documentation categories (ADRs, Recommendations, StyleGuides, Structures)
  - Configurable documentation source (local or repository)

#### Wigo4it.CodeGuidelines.Tests
- **Type**: xUnit Test Project (.NET 10)
- **Purpose**: Unit tests with code coverage
- **Location**: `src/Wigo4it.CodeGuidelines.Tests/`
- **Key Features**:
  - Uses xUnit for testing framework
  - Uses Moq for mocking dependencies
  - Uses coverlet for code coverage (91%+ coverage achieved)
  - Coverage threshold: 80% line coverage required

### Documentation
- All documentation is stored in the `docs/` folder
- Documentation is organized into 4 categories:
  1. **ADRs** - Architecture Decision Records (`docs/ADRs/`)
  2. **Recommendations** - Best practices and guidelines (`docs/Recommendations/`)
  3. **Style Guides** - Coding style and formatting standards (`docs/StyleGuides/`)
  4. **Structures** - Project and code organization patterns (`docs/Structures/`)

## Development Guidelines

### .NET Project Creation
- Use .NET 10 SDK for all projects
- Target C# 14 language features
- Place all projects under `src/` directory
- Use the solution file: `src/wigo4it-code-conventions-mcp.sln`

### Code Style
- Follow modern C# conventions and idioms
- Utilize C# 14 features where appropriate (primary constructors, collection expressions, etc.)
- Apply nullable reference types consistently
- Use file-scoped namespaces
- Prefer `var` for local variables when type is obvious
- Use expression-bodied members for simple methods/properties

### MCP Server Implementation
- Implement MCP protocol specifications correctly using the official C# SDK
- The server is named **wigo4it-code-guidelines**
- Expose documentation resources through MCP tool endpoints:
  - `GetAllDocumentation`: Lists all available documentation with metadata
  - `GetDocumentationByCategory`: Filters documentation by category (ADRs, Recommendations, StyleGuides, Structures)
  - `GetDocumentationContent`: Retrieves full markdown content of a specific document
- Support reading and serving content from the `docs/` folder
- Organize resources by documentation categories (ADRs, Recommendations, Style Guides, Structures)
- Implement proper error handling and logging
- Follow async/await patterns throughout
- Use stdio transport for local development and testing
- Configuration options:
  - `Documentation:UseLocalFileSystem`: Toggle between local filesystem (debug) and repository source
  - `Documentation:BasePath`: Override default documentation path

### Running the MCP Server
```bash
# Run the server locally
dotnet run --project src/Wigo4it.CodeGuidelines.McpServer

# Configure in VS Code's mcp.json
{
  "servers": {
    "wigo4it-code-guidelines": {
      "type": "stdio",
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "<absolute-path-to-repo>/src/Wigo4it.CodeGuidelines.McpServer/Wigo4it.CodeGuidelines.McpServer.csproj"
      ]
    }
  }
}
```

### Documentation Resources
When implementing resource handlers:
- Read markdown files from `docs/` subdirectories
- Parse frontmatter and metadata appropriately
- Maintain category organization (ADRs, Recommendations, Style Guides, Structures)
- Support searching and filtering documentation
- Cache documentation content when appropriate

### Testing
- Write unit tests for core functionality using xUnit
- Test MCP protocol compliance
- Validate documentation resource access
- Mock file system access and dependencies in tests using Moq
- Maintain at least 80% line coverage (currently at 91%+)
- Run tests: `dotnet test src/wigo4it-code-conventions-mcp.sln`
- Run tests with coverage: `dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:Threshold=80 /p:ThresholdType=line`

### Error Handling
- Use structured logging
- Provide clear error messages
- Handle file not found scenarios gracefully
- Validate MCP protocol messages

### Dependencies
- Keep dependencies minimal and up-to-date
- Use official MCP SDK/libraries when available
- Follow .NET dependency injection patterns
- Configure services in Program.cs
- Key packages:
  - `ModelContextProtocol` (prerelease): Official MCP C# SDK
  - `Microsoft.Extensions.Hosting`: For hosting the MCP server
  - `xUnit`: Testing framework
  - `Moq`: Mocking framework for tests
  - `coverlet.msbuild` & `coverlet.collector`: Code coverage tools

### Git Workflow
- This is a private repository
- Write clear, descriptive commit messages
- Keep commits focused and atomic
- Reference issue numbers when applicable

## Common Tasks

### Adding a New Project
```bash
dotnet new <template> -n <ProjectName> -o src/<ProjectName>
dotnet sln src/wigo4it-code-conventions-mcp.sln add src/<ProjectName>/<ProjectName>.csproj
```

### Building the Solution
```bash
dotnet build src/wigo4it-code-conventions-mcp.sln
```

### Running the MCP Server
```bash
dotnet run --project src/<ServerProjectName>
```

## Additional Context

- The MCP server serves as a bridge between documentation consumers and the technical documentation repository
- Documentation should remain in markdown format in the `docs/` folder
- The server should dynamically read and serve documentation without requiring rebuild
- Consider implementing file watching for documentation updates
- Ensure proper content-type handling for markdown resources
