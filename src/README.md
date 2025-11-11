# Wigo4it Coding Guidelines MCP Server

A Model Context Protocol (MCP) server built with .NET 9 that provides coding standards, style guides, and Architecture Decision Records (ADRs) to AI assistants and development tools.

## Features

- **Coding Guidelines**: Access comprehensive coding standards and best practices
- **Style Guides**: Get language-specific formatting and naming conventions
- **ADRs**: Query architectural decisions and their context
- **MCP Integration**: Seamless integration with GitHub Copilot and other MCP clients

## Projects

- **Wigo4it.CodingGuidelines.Core**: Core library containing models and services
- **Wigo4it.CodingGuidelines.McpServer**: MCP server console application
- **Wigo4it.CodingGuidelines.Tests**: xUnit test project

## Getting Started

### Prerequisites

- .NET 9 SDK or later
- Visual Studio Code with GitHub Copilot (for testing)

### Building

```bash
cd src
dotnet build wigo4it-coding-conventions.sln
```

### Running Tests

```bash
cd src
dotnet test wigo4it-coding-conventions.sln
```

### Running the MCP Server

```bash
cd src/Wigo4it.CodingGuidelines.McpServer
dotnet run
```

## VS Code Configuration

To use this MCP server with GitHub Copilot in VS Code, add the following to your `.vscode/mcp.json` file:

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
                "C:\\Projects\\github.com\\Wigo4it\\wigo4it-code-conventions-mcp\\src\\Wigo4it.CodingGuidelines.McpServer\\Wigo4it.CodingGuidelines.McpServer.csproj"
            ]
        }
    }
}
```

Replace the path with the actual path to your project.

## Available Tools

### Coding Guidelines Tools

- `GetAllCodingGuidelines`: Get all coding guidelines
- `GetCodingGuidelineById`: Get a specific guideline by ID (e.g., CG001)
- `GetCodingGuidelinesByCategory`: Filter guidelines by category (e.g., Naming, Design)
- `GetCodingGuidelinesByLanguage`: Filter guidelines by programming language

### Style Guide Tools

- `GetAllStyleGuides`: Get all style guides
- `GetStyleGuideById`: Get a specific style guide by ID (e.g., SG001)
- `GetStyleGuideByLanguage`: Get style guide for a specific language (e.g., C#, TypeScript)

### ADR Tools

- `GetAllADRs`: Get all Architecture Decision Records
- `GetADRById`: Get a specific ADR by ID (e.g., ADR001)
- `GetADRsByStatus`: Filter ADRs by status (Proposed, Accepted, Deprecated, Superseded)

## Example Usage with GitHub Copilot

Once configured, you can ask GitHub Copilot questions like:

- "What are the naming conventions for C#?"
- "Show me the coding guideline for async methods"
- "Get the ADR about microservices architecture"
- "What's the TypeScript style guide?"

## Extending the Server

To add more guidelines, style guides, or ADRs:

1. Edit the initialization methods in `Wigo4it.CodingGuidelines.Core/Services/GuidelinesService.cs`
2. Add new entries to the respective collections
3. Rebuild the project

## License

See LICENSE file for details.
