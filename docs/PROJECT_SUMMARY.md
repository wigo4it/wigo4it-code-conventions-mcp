# Wigo4it Coding Guidelines MCP Server - Project Summary

## Overview
This project implements a Model Context Protocol (MCP) server for Wigo4it's coding guidelines, style guides, and Architecture Decision Records (ADRs) using .NET 9.

## Project Structure

```
src/
├── wigo4it-coding-conventions.sln          # Solution file
├── README.md                                # Project documentation
├── Wigo4it.CodingGuidelines.Core/          # Core library
│   ├── Models/
│   │   ├── CodingGuideline.cs              # Coding guideline model
│   │   ├── StyleGuide.cs                   # Style guide model
│   │   └── ArchitectureDecisionRecord.cs   # ADR model
│   └── Services/
│       └── GuidelinesService.cs            # Service with sample data
├── Wigo4it.CodingGuidelines.McpServer/     # MCP Server application
│   ├── Program.cs                          # MCP server host
│   └── Tools/
│       ├── CodingGuidelinesTools.cs        # Coding guidelines tools
│       ├── StyleGuideTools.cs              # Style guide tools
│       └── ADRTools.cs                     # ADR tools
└── Wigo4it.CodingGuidelines.Tests/         # Unit tests
    └── GuidelinesServiceTests.cs           # Service tests (11 tests, all passing)
```

## Key Features

### MCP Tools Exposed

**Coding Guidelines (4 tools)**
- `GetAllCodingGuidelines` - Retrieve all coding guidelines
- `GetCodingGuidelineById` - Get specific guideline by ID
- `GetCodingGuidelinesByCategory` - Filter by category (Naming, Design, etc.)
- `GetCodingGuidelinesByLanguage` - Filter by programming language

**Style Guides (3 tools)**
- `GetAllStyleGuides` - Retrieve all style guides
- `GetStyleGuideById` - Get specific style guide by ID
- `GetStyleGuideByLanguage` - Get style guide for a language

**Architecture Decision Records (3 tools)**
- `GetAllADRs` - Retrieve all ADRs
- `GetADRById` - Get specific ADR by ID
- `GetADRsByStatus` - Filter by status (Proposed, Accepted, Deprecated, Superseded)

## Sample Data Included

### Coding Guidelines
- CG001: Use Meaningful Variable Names (C#)
- CG002: PascalCase for Class Names (C#)
- CG003: Single Responsibility Principle (General)
- CG004: Async Methods Should End with Async (C#)

### Style Guides
- SG001: C# Coding Style
- SG002: TypeScript Coding Style

### ADRs
- ADR001: Use Model Context Protocol for AI Integration
- ADR002: Adopt Microservices Architecture
- ADR003: Use Repository Pattern for Data Access

## Technology Stack
- **.NET 9**: Latest .NET framework
- **ModelContextProtocol SDK**: Official C# MCP SDK (preview)
- **Microsoft.Extensions.Hosting**: For hosting the MCP server
- **xUnit**: For unit testing
- **Moq**: For mocking in tests

## Testing
All 11 unit tests pass successfully, covering:
- Getting all items
- Getting by ID (valid and invalid)
- Filtering by category, language, and status

## VS Code Integration
Configure in `.vscode/mcp.json` to use with GitHub Copilot. Example configuration provided in `docs/mcp-config-example.json`.

## Next Steps
1. Add more coding guidelines, style guides, and ADRs
2. Consider loading data from external files (JSON, Markdown)
3. Add support for searching by tags
4. Implement versioning for guidelines
5. Add support for custom company-specific rules
6. Consider SSE transport for remote deployment

## Build Status
✅ Solution builds successfully
✅ All tests pass (11/11)
✅ Ready for use with MCP clients
