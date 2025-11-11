# Wigo4it Coding Guidelines MCP Server

An [MCP (Model Context Protocol)](https://modelcontextprotocol.io) server that provides Wigo4it's coding guidelines, style guides, Architecture Decision Records (ADRs), and best practice recommendations to AI assistants like GitHub Copilot.

## 📋 Overview

This MCP server makes Wigo4it's coding standards and documentation accessible to AI-powered development tools through a standardized protocol. It exposes seven tools that allow AI assistants to query and retrieve documentation on-demand during development.

**Key Features:**
- 🔍 **7 MCP Tools** for querying guidelines, styles, ADRs, and recommendations
- 📂 **Dual Source Support** - Load from local filesystem (development) or GitHub (production)
- 🤖 **AI-Ready** - Seamless integration with GitHub Copilot, Claude, and other MCP clients
- 🏷️ **Semantic Search** - Search by type, category, language, or keywords
- 🔄 **Automatic Discovery** - No manual indexing required, documents are discovered automatically

## 🚀 Installation

### Prerequisites

- .NET 9.0 SDK or later
- GitHub account with access to the Wigo4it organization

### Install as a .NET Global Tool

1. **Authenticate with GitHub Packages** (one-time setup):

   ```bash
   dotnet nuget add source "https://nuget.pkg.github.com/wigo4it/index.json" \
     --name "Wigo4it GitHub Packages" \
     --username YOUR_GITHUB_USERNAME \
     --password YOUR_GITHUB_PAT \
     --store-password-in-clear-text
   ```

   Replace `YOUR_GITHUB_USERNAME` with your GitHub username and `YOUR_GITHUB_PAT` with a Personal Access Token that has `read:packages` permission.

2. **Install the tool globally**:

   ```bash
   dotnet tool install --global Wigo4it.CodingGuidelines.McpServer
   ```

3. **Verify installation**:

   ```bash
   wigo4it-guidelines-mcp --version
   ```

### Update to Latest Version

```bash
dotnet tool update --global Wigo4it.CodingGuidelines.McpServer
```

### Uninstall

```bash
dotnet tool uninstall --global Wigo4it.CodingGuidelines.McpServer
```

## 🔧 Configuration

### Visual Studio Code

Configure the MCP server in VS Code for use with GitHub Copilot:

1. **Create or edit `.vscode/mcp.json`** in your workspace:

   ```json
   {
     "servers": {
       "wigo4it-guidelines": {
         "command": "wigo4it-guidelines-mcp",
         "args": []
       }
     }
   }
   ```

2. **For development with local documents** (when working on the guidelines repository):

   ```json
   {
     "servers": {
       "wigo4it-guidelines": {
         "command": "dotnet",
         "args": [
           "run",
           "--project",
           "C:\\path\\to\\wigo4it-code-conventions-mcp\\src\\Wigo4it.CodingGuidelines.McpServer"
         ]
       }
     }
   }
   ```

3. **Reload VS Code** or restart the GitHub Copilot extension

4. **Verify the connection**:
   - Open GitHub Copilot Chat
   - Type: `@workspace What are the C# naming conventions?`
   - Copilot will use the MCP server to retrieve guidelines

### Visual Studio 2022

Visual Studio support for MCP is currently in preview. Configuration varies by version:

#### Using GitHub Copilot Chat (Preview)

1. **Install GitHub Copilot extension** for Visual Studio 2022 (v17.8+)

2. **Configure MCP settings**:
   - Open **Tools > Options > GitHub Copilot > MCP Servers**
   - Add a new MCP server configuration:
     - **Name**: `Wigo4it Guidelines`
     - **Command**: `wigo4it-guidelines-mcp`
     - **Arguments**: (leave empty)

3. **Restart Visual Studio**

4. **Use in Copilot Chat**:
   ```
   What are the Wigo4it naming conventions for C# classes?
   Show me the ADR about using MCP for AI integration
   ```

#### Alternative: Use Claude Desktop with Visual Studio

1. **Install [Claude Desktop](https://claude.ai/download)**

2. **Configure MCP** in Claude (`%APPDATA%\Claude\claude_desktop_config.json`):

   ```json
   {
     "servers": {
       "wigo4it-guidelines": {
         "command": "wigo4it-guidelines-mcp"
       }
     }
   }
   ```

3. **Use Claude for code reviews and guidance** while working in Visual Studio

### Cursor IDE

1. **Open Cursor Settings** (Ctrl+,)

2. **Navigate to Features > MCP**

3. **Add MCP Server**:
   ```json
   {
     "wigo4it-guidelines": {
       "command": "wigo4it-guidelines-mcp"
     }
   }
   ```

4. **Restart Cursor**

### Claude Desktop

Add to `claude_desktop_config.json`:

**Windows**: `%APPDATA%\Claude\claude_desktop_config.json`
**macOS**: `~/Library/Application Support/Claude/claude_desktop_config.json`
**Linux**: `~/.config/Claude/claude_desktop_config.json`

```json
{
  "servers": {
    "wigo4it-guidelines": {
      "command": "wigo4it-guidelines-mcp"
    }
  }
}
```

## 📚 Available MCP Tools

The server exposes 7 tools for querying documentation:

| Tool | Description | Parameters |
|------|-------------|------------|
| `GetAllDocuments` | List all available documents with summaries | None |
| `GetDocumentById` | Retrieve a specific document by its ID | `id` (string) |
| `GetDocumentByPath` | Retrieve a document by its file path | `path` (string) |
| `GetDocumentsByType` | Filter documents by type | `type` (CodingGuideline, StyleGuide, ADR, Recommendation) |
| `GetDocumentsByCategory` | Filter documents by category | `category` (e.g., "naming", "async") |
| `GetDocumentsByLanguage` | Filter documents by programming language | `language` (e.g., "C#", "TypeScript") |
| `SearchDocuments` | Full-text search across all documents | `searchTerm` (string) |

## 💡 Usage Examples

### Example Queries for AI Assistants

When configured with GitHub Copilot, Claude, or other MCP clients, you can ask:

**General Questions:**
- "What are the Wigo4it coding guidelines?"
- "Show me all available documentation"
- "List all ADRs"

**Language-Specific:**
- "What are the C# naming conventions?"
- "Show me TypeScript style guidelines"
- "What are the best practices for async/await in C#?"

**Search and Discovery:**
- "Find documents about dependency injection"
- "Show recommendations for API design"
- "What's the ADR about observability?"

**Specific Documents:**
- "Get the style guide for C#"
- "Show the ADR about using Aspire"
- "Display the naming conventions for variables"

### Direct Tool Invocation (for testing)

You can test the MCP server directly using an MCP inspector tool:

```bash
# Install MCP Inspector (if not already installed)
npm install -g @modelcontextprotocol/inspector

# Run inspector
mcp-inspector wigo4it-guidelines-mcp
```

Then invoke tools through the inspector interface:

```json
// GetAllDocuments
{}

// GetDocumentsByType
{
  "type": "CodingGuideline"
}

// SearchDocuments
{
  "searchTerm": "naming"
}
```

## 📖 Document Structure

Documents are organized in the following structure:

```
docs/
├── guidelines/        # Coding guidelines (naming, patterns, etc.)
├── styles/           # Style guides (formatting, structure)
├── adr/              # Architecture Decision Records
└── recommendations/  # Best practices and recommendations
```

Each document contains:
- **Title**: Descriptive name
- **Type**: CodingGuideline, StyleGuide, ADR, or Recommendation
- **Category**: Topic category (e.g., "naming", "async")
- **Language**: Programming language (e.g., "C#", "TypeScript")
- **Tags**: Keywords for search
- **Content**: Full markdown documentation

## 🔍 How It Works

### Environment Detection

The MCP server automatically detects its running environment:

- **Local Development**: Loads documents from `docs/` folder in the repository
- **Production (Installed Tool)**: Loads documents from GitHub via API

### Document Discovery

Documents are discovered automatically:
1. **Filesystem**: Recursively scans `docs/` directory for `*.md` files
2. **GitHub**: Queries GitHub Contents API to fetch document tree
3. **Parsing**: Extracts metadata from markdown files (title, language, tags)
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
   cd src/Wigo4it.CodingGuidelines.McpServer
   dotnet run
   ```

3. **Run tests**:
   ```bash
   cd src
   dotnet test
   ```

### Adding New Documents

1. **Create a markdown file** in the appropriate `docs/` subfolder:
   - `docs/guidelines/` - Coding guidelines
   - `docs/styles/` - Style guides
   - `docs/adr/` - Architecture Decision Records
   - `docs/recommendations/` - Best practices

2. **Include metadata** (optional but recommended):
   ```markdown
   # Document Title

   Language: C#
   Category: Naming
   Tags: classes, naming, conventions

   ## Content
   ...
   ```

3. **Commit and push** to `main` branch

4. **Automatic publishing**: GitHub Actions will build and publish a new version

### Project Structure

```
src/
├── Wigo4it.CodingGuidelines.Core/        # Core library
│   ├── Models/                           # Document models
│   ├── Configuration/                    # Configuration classes
│   ├── Loaders/                          # Document loading strategies
│   └── Services/                         # Business logic
│
├── Wigo4it.CodingGuidelines.McpServer/   # MCP Server host
│   ├── Tools/                            # MCP tool definitions
│   └── Program.cs                        # Entry point
│
└── Wigo4it.CodingGuidelines.Tests/       # Unit tests
```

## 📦 Publishing

The project uses GitHub Actions for automated publishing:

- **Trigger**: Push to `main` branch
- **Versioning**: GitVersion with semantic versioning
- **Output**: Two NuGet packages published to GitHub Packages
  - `Wigo4it.CodingGuidelines.Core` - Core library
  - `Wigo4it.CodingGuidelines.McpServer` - .NET Global Tool

### Version Control

Versions are controlled via commit messages:

```bash
# Patch version (1.0.0 → 1.0.1)
git commit -m "fix: correct typo in naming guidelines"

# Minor version (1.0.0 → 1.1.0)
git commit -m "feat: add TypeScript style guide +semver: minor"

# Major version (1.0.0 → 2.0.0)
git commit -m "feat!: redesign document structure +semver: major"

# No version change
git commit -m "docs: update README +semver: none"
```

## 🤝 Contributing

Contributions are welcome! Please follow these guidelines:

1. **Fork the repository**
2. **Create a feature branch**: `git checkout -b feature/my-new-guideline`
3. **Add your documentation** to the appropriate `docs/` folder
4. **Commit with semantic message**: `git commit -m "feat: add Python naming conventions +semver: minor"`
5. **Push to your fork**: `git push origin feature/my-new-guideline`
6. **Create a Pull Request**

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

- ✅ Automated publishing with GitHub Actions
- ✅ Semantic versioning with GitVersion
- ✅ Full XML documentation
- ✅ Unit test coverage
- ✅ Local and GitHub document sources
- ✅ 7 MCP tools for document access

---

**Made with ❤️ by Wigo4it**
