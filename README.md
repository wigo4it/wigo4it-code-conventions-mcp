# Wigo4it Code Guidelines MCP Server

An [MCP (Model Context Protocol)](https://modelcontextprotocol.io) server that provides access to Wigo4it's code guidelines, Architecture Decision Records (ADRs), recommendations, style guides, and project structures to AI assistants like GitHub Copilot.

## 📋 Overview

This MCP server makes Wigo4it's coding standards and documentation accessible to AI-powered development tools through the Model Context Protocol. It exposes three tools that allow AI assistants to query and retrieve documentation on-demand during development.

**Key Features:**
- 🔍 **3 MCP Tools** for querying guidelines, ADRs, recommendations, style guides, and structures
- 📂 **Dual Source Support** - Load from local filesystem (development) or GitHub (production)
- 🤖 **AI-Ready** - Seamless integration with GitHub Copilot, Claude, and other MCP clients
- 🏷️ **Category-Based Organization** - ADRs, Recommendations, StyleGuides, Structures
- 🔄 **Automatic Discovery** - Documents are discovered automatically from GitHub or local filesystem
- 🚀 **Public Repository** - No authentication required, works out of the box

## 🚀 Installation

### Prerequisites

- .NET 10.0 SDK or later
- An MCP-compatible client (VS Code with GitHub Copilot, Claude Desktop, Cursor, etc.)

### Option 1: Install as a .NET Global Tool (Recommended)

Coming soon! Once published to NuGet.org or GitHub Packages, you'll be able to install with:

```bash
dotnet tool install --global Wigo4it.CodeGuidelines.McpServer
```

### Option 2: Run from Source (Current Method)

1. **Clone the repository**:
   ```bash
   git clone https://github.com/wigo4it/wigo4it-code-conventions-mcp.git
   cd wigo4it-code-conventions-mcp
   ```

2. **Build the project**:
   ```bash
   dotnet build src/wigo4it-code-conventions-mcp.sln
   ```

3. **Verify it works**:
   ```bash
   dotnet run --project src/Wigo4it.CodeGuidelines.McpServer
   ```
   The server will start and wait for stdio connections from an MCP client.

## 🔧 Configuration

### Visual Studio Code

Configure the MCP server in VS Code for use with GitHub Copilot:

1. **Create or edit `.vscode/mcp.json`** in your workspace:

   ```json
   {
     "servers": {
       "wigo4it-code-guidelines": {
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "C:\\absolute\\path\\to\\wigo4it-code-conventions-mcp\\src\\Wigo4it.CodeGuidelines.McpServer"
         ]
       }
     }
   }
   ```

   **Important:** Replace the path with the absolute path to where you cloned the repository.

2. **Reload VS Code** or restart the GitHub Copilot extension

3. **Verify the connection**:
   - Open GitHub Copilot Chat
   - The MCP tools should be available automatically
   - Try asking: `What ADRs are available?`

### Claude Desktop

Add to `claude_desktop_config.json`:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`  
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`  
**Linux**: `~/.config/Claude/claude_desktop_config.json`

```json
{
  "mcpServers": {
    "wigo4it-code-guidelines": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "C:\\absolute\\path\\to\\wigo4it-code-conventions-mcp\\src\\Wigo4it.CodeGuidelines.McpServer"
      ]
    }
  }
}
```

**Important:** Replace the path with the absolute path to where you cloned the repository.

## 📚 Available MCP Tools

The server exposes 3 tools for querying documentation:

| Tool | Description | Parameters |
|------|-------------|------------|
| `GetAllDocumentation` | List all available documents with metadata (title, category, description) | None |
| `GetDocumentationByCategory` | Filter documents by category | `category` (ADRs, Recommendations, StyleGuides, Structures) |
| `GetDocumentationContent` | Retrieve full markdown content of a specific document | `id` (format: "category/filename") |

### Categories

- **ADRs** - Architecture Decision Records (e.g., ADR-001, ADR-002, ADR-003)
- **Recommendations** - Best practices and recommendations (e.g., Aspire usage)
- **StyleGuides** - Coding style guides (e.g., C# style guide)
- **Structures** - Project structure and organization guidelines

## 💡 Usage Examples

### Example Queries for AI Assistants

When configured with GitHub Copilot, Claude, or other MCP clients, you can ask:

**General Questions:**
- "What documentation is available?"
- "Show me all ADRs"
- "List all style guides"

**Category-Specific:**
- "Show me all recommendations"
- "What ADRs exist for architecture decisions?"
- "Display all project structures"

**Specific Documents:**
- "Show me the C# style guide"
- "Get the ADR about modular monoliths"
- "Display the .NET project structure"
- "Show the recommendation about Aspire"

### Tool Usage Examples

The MCP server exposes tools that AI assistants use automatically. Here are examples of how the tools work:

#### GetAllDocumentation
Returns a list of all available documentation with metadata:
```json
{
  "tool": "GetAllDocumentation",
  "parameters": {}
}
```

#### GetDocumentationByCategory
Get documents in a specific category:
```json
{
  "tool": "GetDocumentationByCategory",
  "parameters": {
    "category": "ADRs"
  }
}
```

#### GetDocumentationContent
Retrieve the full content of a document:
```json
{
  "tool": "GetDocumentationContent",
  "parameters": {
    "id": "adrs/adr-003-prefer-modular-monoliths"
  }
}
```

## 📖 Document Structure

Documents are organized in the following structure:

```
docs/
├── ADRs/              # Architecture Decision Records
│   ├── ADR-001-migration-to-dotnet-10.md
│   ├── ADR-002-adoption-of-aspire-for-distributed-applications.md
│   └── ADR-003-prefer-modular-monoliths.md
├── Recommendations/   # Best practices and recommendations
│   └── aspire-embrace.md
├── StyleGuides/       # Coding style guides
│   └── csharp-style-guide.md
└── Structures/        # Project structure guidelines
    └── dotnet-project-structure.md
```

Each document is a markdown file that may contain:
- **Frontmatter**: YAML metadata (optional)
- **Title**: Extracted from the first `# Heading`
- **Content**: Full markdown documentation
- **Description**: Extracted from the first paragraph

### Document ID Format

Document IDs follow the format: `category/filename` (without `.md` extension)

Examples:
- `adrs/adr-001-migration-to-dotnet-10`
- `styleguides/csharp-style-guide`
- `structures/dotnet-project-structure`
- `recommendations/aspire-embrace`

## 🔍 How It Works

### Dual-Source Architecture

The MCP server supports two modes for loading documentation:

1. **GitHub Mode (Default)** - For production use
   - Fetches documentation directly from the public GitHub repository
   - Uses GitHub Contents API to list files and Raw Content API for content
   - No local clone required
   - Automatically caches documents in memory for performance
   - Rate limit: 60 requests per hour (unauthenticated)

2. **Local File System Mode** - For development
   - Loads documents from the local `docs/` folder
   - Useful when developing new documentation
   - Faster for rapid iteration
   - No network calls required

### Configuration

The mode is controlled in `appsettings.json`:

```json
{
  "Documentation": {
    "UseLocalFileSystem": false,  // false = GitHub mode (default)
    "GitHubOwner": "wigo4it",
    "GitHubRepository": "wigo4it-code-conventions-mcp",
    "GitHubBranch": "main",
    "DocsPath": "docs"
  }
}
```

For local development, set `UseLocalFileSystem` to `true`.

### Document Discovery

Documents are discovered automatically:
1. **GitHub Mode**: Scans repository using GitHub Contents API
2. **Local Mode**: Recursively scans `docs/` directory for `*.md` files
3. **Parsing**: Extracts title from first `# Heading` and description from first paragraph
4. **Caching**: Documents are cached in memory for fast access

No manual index maintenance is required.

## 🛠️ Development

### Running from Source

1. **Clone the repository**:
   ```bash
   git clone https://github.com/wigo4it/wigo4it-code-conventions-mcp.git
   cd wigo4it-code-conventions-mcp
   ```

2. **Run the MCP server**:
   ```bash
   dotnet run --project src/Wigo4it.CodeGuidelines.McpServer
   ```

3. **Run tests**:
   ```bash
   cd src
   dotnet test
   ```

4. **Build the solution**:
   ```bash
   dotnet build src/wigo4it-code-conventions-mcp.sln
   ```

### Adding New Documents

1. **Create a markdown file** in the appropriate `docs/` subfolder:
   - `docs/ADRs/` - Architecture Decision Records
   - `docs/Recommendations/` - Best practices and recommendations
   - `docs/StyleGuides/` - Coding style guides
   - `docs/Structures/` - Project structure guidelines

2. **Write your documentation** in markdown format:
   ```markdown
   ---
   title: My New Guideline
   category: StyleGuide
   status: Active
   ---

   # My New Guideline

   This is the introduction paragraph that will be used as the description.

   ## Section 1
   ...
   ```

3. **Test locally**:
   - Set `UseLocalFileSystem: true` in `appsettings.json`
   - Run the server and verify your document appears

4. **Commit and push** to `main` branch:
   ```bash
   git add docs/
   git commit -m "docs: add new guideline"
   git push
   ```

The document will be automatically available via GitHub mode after pushing.

### Project Structure

```
src/
├── Wigo4it.CodeGuidelines.Server/        # Core library
│   ├── Models/                           # DocumentationMetadata, Category, Content
│   ├── Configuration/                    # DocumentationOptions
│   ├── Services/                         # IDocumentationService implementations
│   │   ├── GitHubDocumentationService.cs       # GitHub API implementation
│   │   └── LocalFileSystemDocumentationService.cs  # Local file implementation
│   └── Tools/                            # DocumentationTools (MCP tools)
│
├── Wigo4it.CodeGuidelines.McpServer/     # MCP Server host
│   ├── Program.cs                        # Entry point & DI configuration
│   └── appsettings.json                  # Configuration
│
└── Wigo4it.CodeGuidelines.Tests/         # Unit tests (23 tests, 91%+ coverage)
```

## 📦 Publishing

The project is configured as a .NET Global Tool and can be published to NuGet or GitHub Packages.

### Package Configuration

- **Package ID**: `Wigo4it.CodeGuidelines.McpServer`
- **Tool Command Name**: `wigo4it-code-guidelines`
- **Target Framework**: .NET 10.0
- **License**: MIT

### CI/CD with GitHub Actions

The project uses GitHub Actions for automated building and testing:

- **Trigger**: Push to `main` branch or pull requests
- **Jobs**: Build, Test, Coverage
- **Test Coverage**: 91%+ (20 passing tests, 3 skipped integration tests)

### Manual Build & Pack

To create a NuGet package locally:

```bash
# Build the solution
dotnet build src/wigo4it-code-conventions-mcp.sln --configuration Release

# Pack the tool
dotnet pack src/Wigo4it.CodeGuidelines.McpServer/Wigo4it.CodeGuidelines.McpServer.csproj --configuration Release

# The .nupkg file will be in src/Wigo4it.CodeGuidelines.McpServer/nupkg/
```

### Version Control

Versions can be controlled via project properties or GitVersion:

```xml
<PropertyGroup>
  <Version>1.0.0</Version>
</PropertyGroup>
```

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/my-new-guideline`
3. **Add your documentation** to the appropriate `docs/` folder (ADRs, Recommendations, StyleGuides, or Structures)
4. **Test locally** with `UseLocalFileSystem: true`
5. **Commit your changes**: `git commit -m "docs: add new guideline"`
6. **Push to your fork**: `git push origin feature/my-new-guideline`
7. **Create a Pull Request**

### Documentation Guidelines

- Use clear, concise language
- Follow existing document structure and formatting
- Include code examples where appropriate
- Add frontmatter with title, category, and status
- Test that documents load correctly via MCP tools

## 📄 License

This project is licensed under the MIT License. See [LICENSE](LICENSE) file for details.

## 🔗 Related Links

- [Model Context Protocol Documentation](https://modelcontextprotocol.io)
- [GitHub Copilot](https://github.com/features/copilot)
- [Claude Desktop](https://claude.ai/download)
- [Cursor IDE](https://cursor.sh)

## 💬 Support

For questions, issues, or suggestions:

- **Issues**: [GitHub Issues](https://github.com/wigo4it/wigo4it-code-conventions-mcp/issues)
- **Discussions**: [GitHub Discussions](https://github.com/wigo4it/wigo4it-code-conventions-mcp/discussions)
- **Internal**: Contact the Wigo4it development team

## 📊 Project Status

- ✅ .NET 10.0 with C# 14
- ✅ 3 MCP tools for documentation access
- ✅ Dual-source support (GitHub + Local filesystem)
- ✅ 23 unit tests with 91%+ code coverage
- ✅ GitHub and local documentation services
- ✅ Public repository (no authentication required)
- ✅ Configured as .NET Global Tool
- ✅ Full XML documentation
- 🚧 Publishing to NuGet.org (coming soon)

### Current Documentation

- **3 ADRs**: .NET 10 migration, Aspire adoption, Modular monoliths
- **1 Recommendation**: Fully embrace Aspire
- **1 Style Guide**: C# code style guide (Microsoft conventions)
- **1 Structure**: .NET project structure guidelines

---

**Made with ❤️ by Wigo4it**
