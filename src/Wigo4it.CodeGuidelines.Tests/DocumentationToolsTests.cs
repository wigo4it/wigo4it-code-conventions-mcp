using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Text.Json;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;
using Wigo4it.CodeGuidelines.Server.Tools;

namespace Wigo4it.CodeGuidelines.Tests;

public class DocumentationToolsTests
{
    private IDocumentationService CreateTestService()
    {
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        File.WriteAllText(Path.Combine(adrsDir, "test-adr.md"), "# Test ADR\nTest content");

        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = true,
            BasePath = tempDir
        });
        var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
        return new LocalFileSystemDocumentationService(options, logger);
    }

    [Fact]
    public async Task GetAllDocumentation_ReturnsJsonList()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetAllDocumentation(service);

        // Assert
        Assert.NotNull(result);
        var docs = JsonSerializer.Deserialize<List<DocumentationMetadata>>(result);
        Assert.NotNull(docs);
        Assert.Single(docs);
    }

    [Fact]
    public async Task GetDocumentationByCategory_ReturnsFilteredJson()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetDocumentationByCategory(service, "ADRs");

        // Assert
        Assert.NotNull(result);
        var docs = JsonSerializer.Deserialize<List<DocumentationMetadata>>(result);
        Assert.NotNull(docs);
        Assert.Single(docs);
    }

    [Fact]
    public async Task GetDocumentationByCategory_ReturnsError_ForInvalidCategory()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetDocumentationByCategory(service, "InvalidCategory");

        // Assert
        Assert.Contains("Invalid category", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task GetDocumentationContent_ReturnsContent_WhenExists()
    {
        // Arrange
        var service = CreateTestService();
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var result = await DocumentationTools.GetDocumentationContent(service, "adrs/test-adr");

        // Assert
        Assert.NotNull(result);
        Assert.Contains("Test ADR", result);
        Assert.Contains("Test content", result);
    }

    [Fact]
    public async Task GetDocumentationContent_ReturnsError_WhenNotFound()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetDocumentationContent(service, "nonexistent/doc");

        // Assert
        Assert.Contains("not found", result);
        Assert.Contains("error", result);
    }

    [Fact]
    public async Task SearchDocumentation_ReturnsResults_WhenMatchesFound()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Use Aspire\n\nThis document describes Aspire usage.");

        try
        {
            var options = Options.Create(new DocumentationOptions
            {
                UseLocalFileSystem = true,
                BasePath = tempDir
            });
            var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
            var service = new LocalFileSystemDocumentationService(options, logger);

            // Act
            var result = await DocumentationTools.SearchDocumentation(service, "aspire");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("aspire", result, StringComparison.OrdinalIgnoreCase);
            Assert.Contains("resultCount", result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task SearchDocumentation_ReturnsError_ForEmptySearchTerm()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.SearchDocumentation(service, "");

        // Assert
        Assert.Contains("error", result);
        Assert.Contains("cannot be empty", result);
    }

    [Fact]
    public async Task GetRelatedDocumentation_ReturnsRelatedDocs()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Architecture\n\nArchitecture decision about services.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "# ADR-002: Services\n\nServices architecture patterns.");

        try
        {
            var options = Options.Create(new DocumentationOptions
            {
                UseLocalFileSystem = true,
                BasePath = tempDir
            });
            var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
            var service = new LocalFileSystemDocumentationService(options, logger);

            // Act
            var result = await DocumentationTools.GetRelatedDocumentation(service, "adrs/adr-001", 5);

            // Assert
            Assert.NotNull(result);
            Assert.Contains("sourceDocumentId", result);
            Assert.Contains("resultCount", result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetRelatedDocumentation_ReturnsError_ForEmptyDocumentId()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetRelatedDocumentation(service, "", 5);

        // Assert
        Assert.Contains("error", result);
        Assert.Contains("cannot be empty", result);
    }

    [Fact]
    public async Task GetRelatedDocumentation_ReturnsError_ForInvalidMaxResults()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetRelatedDocumentation(service, "adrs/test", 0);

        // Assert
        Assert.Contains("error", result);
        Assert.Contains("must be between", result);
    }

    [Fact]
    public async Task GetDocumentationByTag_ReturnsFilteredDocs()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [architecture, design]\n---\n# ADR-001");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "---\ntags: [testing, quality]\n---\n# ADR-002");

        try
        {
            var options = Options.Create(new DocumentationOptions
            {
                UseLocalFileSystem = true,
                BasePath = tempDir
            });
            var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
            var service = new LocalFileSystemDocumentationService(options, logger);

            // Act
            var result = await DocumentationTools.GetDocumentationByTag(service, "architecture");

            // Assert
            Assert.NotNull(result);
            Assert.Contains("searchTags", result);
            Assert.Contains("resultCount", result);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetDocumentationByTag_ReturnsError_ForEmptyTags()
    {
        // Arrange
        var service = CreateTestService();

        // Act
        var result = await DocumentationTools.GetDocumentationByTag(service, "");

        // Assert
        Assert.Contains("error", result);
        Assert.Contains("cannot be empty", result);
    }
}

