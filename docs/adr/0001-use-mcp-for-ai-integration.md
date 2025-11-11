# Use Model Context Protocol for AI Integration

Category: Architecture
Tags: architecture, ai, integration, mcp

## Status
Accepted

## Date
2025-01-15

## Context

We need a standardized way to provide coding guidelines, architectural decisions, and development standards to AI assistants and developers. Currently, our documentation is scattered across various sources:

- README files in repositories
- Wiki pages
- Confluence documentation
- Tribal knowledge in team members' heads

This makes it difficult for:
1. New team members to onboard
2. AI assistants to provide context-aware suggestions
3. Maintaining consistency across projects
4. Keeping documentation up-to-date

Additionally, AI-powered development tools like GitHub Copilot are becoming essential to our workflow, but they lack access to our specific coding standards and architectural decisions.

## Decision

We will implement a Model Context Protocol (MCP) server using .NET 9 to expose our coding guidelines, style guides, Architecture Decision Records (ADRs), and recommendations through a structured API.

The MCP server will:
1. Load documentation from markdown files in a centralized repository
2. Support both local file system and GitHub as data sources
3. Automatically detect the environment (local development vs. deployed)
4. Provide tools for querying documents by type, category, language, and keywords
5. Integrate seamlessly with GitHub Copilot and other MCP clients

## Implementation Details

### Architecture
- **Core Library**: Domain models and business logic
- **Loaders**: Pluggable document loaders (Local, GitHub)
- **Service Layer**: Document management and caching
- **MCP Server**: Tool implementations and hosting

### Technology Stack
- .NET 9
- Model Context Protocol SDK (C#)
- Microsoft.Extensions.Hosting for server hosting
- HttpClient for GitHub API integration

### Document Structure
```
docs/
├── guidelines/          # Coding guidelines
│   ├── csharp-*.md
│   ├── typescript-*.md
│   └── ...
├── styles/             # Style guides
│   ├── csharp-style-guide.md
│   └── ...
├── adr/                # Architecture Decision Records
│   ├── 0001-use-mcp.md
│   └── ...
└── recommendations/    # Best practice recommendations
    └── ...
```

## Consequences

### Positive

1. **Centralized Documentation**: Single source of truth for all coding standards
2. **AI Integration**: GitHub Copilot and other AI tools have direct access to our standards
3. **Easy Updates**: Documentation updates in markdown immediately available
4. **Version Control**: Git history tracks all changes to standards
5. **Searchable**: Full-text search across all documentation
6. **Extensible**: Easy to add new document types or data sources
7. **Consistent Experience**: Same interface across local and deployed environments

### Negative

1. **Additional Infrastructure**: Need to maintain and deploy the MCP server
2. **Learning Curve**: Team needs to learn MCP concepts and tools
3. **Dependency**: Reliance on external MCP protocol and SDK
4. **GitHub API Limits**: Rate limiting when using GitHub as data source
5. **Markdown Constraints**: Documentation must be in markdown format

### Neutral

1. **Migration Effort**: Existing documentation needs to be migrated to markdown
2. **Tooling Setup**: Developers need to configure VS Code with MCP settings
3. **Maintenance**: Documentation needs regular review and updates

## Alternatives Considered

### 1. Static Site Generator (e.g., Docusaurus, MkDocs)
**Pros**: Beautiful UI, search, versioning
**Cons**: No AI integration, requires deployment, slower to query

### 2. Confluence Integration
**Pros**: Already using Confluence, familiar to team
**Cons**: No AI integration, requires API key management, slower

### 3. README-only Approach
**Pros**: Simple, no infrastructure
**Cons**: Scattered, hard to search, no AI integration

### 4. Custom API
**Pros**: Full control
**Cons**: More development effort, not compatible with existing tools

## Implementation Plan

1. ✅ Create MCP server project structure
2. ✅ Implement document loaders (Local and GitHub)
3. ✅ Implement MCP tools for document access
4. ✅ Add configuration and environment detection
5. 🔄 Migrate existing documentation to markdown
6. 🔄 Deploy MCP server for team use
7. 🔄 Document setup process for developers
8. 🔄 Train team on usage

## Review Date

This decision should be reviewed in 6 months (July 2025) to assess:
- Adoption by team members
- Integration with AI tools
- Maintenance burden
- Value delivered

## References

- [Model Context Protocol Documentation](https://modelcontextprotocol.io/)
- [MCP C# SDK](https://github.com/modelcontextprotocol/csharp-sdk)
- [GitHub Copilot MCP Integration](https://code.visualstudio.com/docs/copilot/copilot-extensibility-overview)
