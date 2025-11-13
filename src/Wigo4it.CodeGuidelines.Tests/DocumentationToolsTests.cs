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
        var logger = Mock.Of<ILogger<DocumentationService>>();
        return new DocumentationService(options, logger);
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
}
