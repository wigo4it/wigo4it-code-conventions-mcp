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

### Documentation
- All documentation is stored in the `docs/` folder
- Documentation is organized into 4 categories:
  1. **ADRs** - Architecture Decision Records
  2. **Recommendations** - Best practices and guidelines
  3. **Style Guides** - Coding style and formatting standards
  4. **Structures** - Project and code organization patterns

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
- Implement MCP protocol specifications correctly
- Expose documentation resources through MCP resource endpoints
- Support reading and serving content from the `docs/` folder
- Organize resources by documentation categories (ADRs, Recommendations, Style Guides, Structures)
- Implement proper error handling and logging
- Follow async/await patterns throughout

### Documentation Resources
When implementing resource handlers:
- Read markdown files from `docs/` subdirectories
- Parse frontmatter and metadata appropriately
- Maintain category organization (ADRs, Recommendations, Style Guides, Structures)
- Support searching and filtering documentation
- Cache documentation content when appropriate

### Testing
- Write unit tests for core functionality
- Test MCP protocol compliance
- Validate documentation resource access
- Mock file system access in tests

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
