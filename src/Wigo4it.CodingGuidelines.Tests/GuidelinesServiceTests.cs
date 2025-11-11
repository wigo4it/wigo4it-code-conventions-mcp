using Wigo4it.CodingGuidelines.Core.Configuration;
using Wigo4it.CodingGuidelines.Core.Loaders;
using Wigo4it.CodingGuidelines.Core.Services;
using Xunit;

namespace Wigo4it.CodingGuidelines.Tests;

public class DocumentServiceTests
{
    private readonly DocumentService _service;
    private static readonly string ProjectRoot = GetProjectRoot();

    public DocumentServiceTests()
    {
        // Setup local document loader for tests
        var configuration = new DocumentSourceConfiguration
        {
            SourceType = DocumentSourceType.Local,
            LocalBasePath = ProjectRoot,
            DocsPath = "docs"
        };

        var loader = new LocalDocumentLoader(configuration);
        _service = new DocumentService(loader);
    }

    [Fact]
    public async Task GetAllDocuments_ShouldReturnDocuments()
    {
        // Act
        var documents = await _service.GetAllDocumentsAsync();

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
    }

    [Fact]
    public async Task GetAllDocumentSummaries_ShouldReturnSummaries()
    {
        // Act
        var summaries = await _service.GetAllDocumentSummariesAsync();

        // Assert
        Assert.NotNull(summaries);
        Assert.NotEmpty(summaries);
        Assert.All(summaries, s =>
        {
            Assert.NotNull(s.Id);
            Assert.NotNull(s.Title);
            Assert.NotNull(s.Type);
        });
    }

    [Fact]
    public async Task GetDocumentsByType_WithCodingGuideline_ShouldReturnFilteredDocuments()
    {
        // Arrange
        var type = "CodingGuideline";

        // Act
        var documents = await _service.GetDocumentsByTypeAsync(type);

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
        Assert.All(documents, d => Assert.Equal(type, d.Type));
    }

    [Fact]
    public async Task GetDocumentsByType_WithStyleGuide_ShouldReturnFilteredDocuments()
    {
        // Arrange
        var type = "StyleGuide";

        // Act
        var documents = await _service.GetDocumentsByTypeAsync(type);

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
        Assert.All(documents, d => Assert.Equal(type, d.Type));
    }

    [Fact]
    public async Task GetDocumentsByType_WithADR_ShouldReturnFilteredDocuments()
    {
        // Arrange
        var type = "ADR";

        // Act
        var documents = await _service.GetDocumentsByTypeAsync(type);

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
        Assert.All(documents, d => Assert.Equal(type, d.Type));
    }

    [Fact]
    public async Task GetDocumentsByLanguage_WithCSharp_ShouldReturnFilteredDocuments()
    {
        // Arrange
        var language = "C#";

        // Act
        var documents = await _service.GetDocumentsByLanguageAsync(language);

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
        Assert.All(documents, d => Assert.Equal(language, d.Language));
    }

    [Fact]
    public async Task SearchDocuments_WithKeyword_ShouldReturnMatchingDocuments()
    {
        // Arrange
        var searchTerm = "naming";

        // Act
        var documents = await _service.SearchDocumentsAsync(searchTerm);

        // Assert
        Assert.NotNull(documents);
        Assert.NotEmpty(documents);
    }

    [Fact]
    public async Task GetDocumentByPath_WithValidPath_ShouldReturnDocument()
    {
        // Arrange
        var documents = await _service.GetAllDocumentsAsync();
        if (documents.Count == 0)
        {
            // Skip test if no documents found
            return;
        }
        
        var firstDoc = documents.First();

        // Act
        var document = await _service.GetDocumentByPathAsync(firstDoc.Path);

        // Assert
        Assert.NotNull(document);
        Assert.Equal(firstDoc.Path, document.Path);
    }

    [Fact]
    public async Task GetDocumentByPath_WithInvalidPath_ShouldReturnNull()
    {
        // Arrange
        var invalidPath = "non-existent/path.md";

        // Act
        var document = await _service.GetDocumentByPathAsync(invalidPath);

        // Assert
        Assert.Null(document);
    }

    private static string GetProjectRoot()
    {
        var currentDir = Directory.GetCurrentDirectory();
        var directory = new DirectoryInfo(currentDir);

        while (directory != null)
        {
            // Look for .git directory (true repository root)
            if (Directory.Exists(Path.Combine(directory.FullName, ".git")))
            {
                return directory.FullName;
            }

            // Check if this directory contains a docs folder
            if (Directory.Exists(Path.Combine(directory.FullName, "docs")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new InvalidOperationException("Could not find project root");
    }
}
