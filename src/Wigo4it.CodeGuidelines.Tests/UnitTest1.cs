using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;

namespace Wigo4it.CodeGuidelines.Tests;

public class DocumentationServiceTests
{
    [Fact]
    public async Task GetAllDocumentationAsync_ReturnsEmptyList_WhenNoDocsExist()
    {
        // Arrange
        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = true,
            BasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        });
        var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
        var service = new LocalFileSystemDocumentationService(options, logger);

        // Act
        var result = await service.GetAllDocumentationAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetDocumentationByCategoryAsync_ReturnsFilteredList()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        var testFilePath = Path.Combine(adrsDir, "test-adr.md");
        await File.WriteAllTextAsync(testFilePath, "# Test ADR\n\nThis is a test ADR document.");

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
            var result = await service.GetDocumentationByCategoryAsync(DocumentationCategory.ADRs);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal("Test ADR", result[0].Title);
            Assert.Equal(DocumentationCategory.ADRs, result[0].Category);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetDocumentationContentAsync_ReturnsContent_WhenDocExists()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var recommendationsDir = Path.Combine(tempDir, "Recommendations");
        Directory.CreateDirectory(recommendationsDir);

        var testContent = "# Test Recommendation\n\nThis is test content for recommendations.";
        var testFilePath = Path.Combine(recommendationsDir, "test-rec.md");
        await File.WriteAllTextAsync(testFilePath, testContent);

        try
        {
            var options = Options.Create(new DocumentationOptions
            {
                UseLocalFileSystem = true,
                BasePath = tempDir
            });
            var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
            var service = new LocalFileSystemDocumentationService(options, logger);

            // Force initialization
            await service.GetAllDocumentationAsync();

            // Act
            var result = await service.GetDocumentationContentAsync("recommendations/test-rec");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Test Recommendation", result.Metadata.Title);
            Assert.Contains("This is test content", result.Content);
        }
        finally
        {
            // Cleanup
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    [Fact]
    public async Task GetDocumentationContentAsync_ReturnsNull_WhenDocDoesNotExist()
    {
        // Arrange
        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = true,
            BasePath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString())
        });
        var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
        var service = new LocalFileSystemDocumentationService(options, logger);

        // Act
        var result = await service.GetDocumentationContentAsync("nonexistent/doc");

        // Assert
        Assert.Null(result);
    }
}
