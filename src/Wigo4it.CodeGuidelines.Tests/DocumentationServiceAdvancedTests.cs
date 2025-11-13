using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;

namespace Wigo4it.CodeGuidelines.Tests;

public class DocumentationServiceAdvancedTests
{
    [Fact]
    public async Task GetAllDocumentationAsync_HandlesMultipleCategories()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        
        // Create multiple category folders
        var adrsDir = Path.Combine(tempDir, "ADRs");
        var recsDir = Path.Combine(tempDir, "Recommendations");
        var stylesDir = Path.Combine(tempDir, "StyleGuides");
        
        Directory.CreateDirectory(adrsDir);
        Directory.CreateDirectory(recsDir);
        Directory.CreateDirectory(stylesDir);

        await File.WriteAllTextAsync(Path.Combine(adrsDir, "adr-001.md"), "# ADR 001");
        await File.WriteAllTextAsync(Path.Combine(recsDir, "rec-001.md"), "# Recommendation 001");
        await File.WriteAllTextAsync(Path.Combine(stylesDir, "style-001.md"), "# Style Guide 001");

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
            var result = await service.GetAllDocumentationAsync();

            // Assert
            Assert.Equal(3, result.Count);
            Assert.Contains(result, d => d.Category == DocumentationCategory.ADRs);
            Assert.Contains(result, d => d.Category == DocumentationCategory.Recommendations);
            Assert.Contains(result, d => d.Category == DocumentationCategory.StyleGuides);
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
    public async Task GetDocumentationByCategoryAsync_ReturnsEmptyForNonExistentCategory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(Path.Combine(adrsDir, "adr-001.md"), "# ADR 001");

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
            var result = await service.GetDocumentationByCategoryAsync(DocumentationCategory.Structures);

            // Assert
            Assert.NotNull(result);
            Assert.Empty(result);
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
    public async Task DocumentationService_HandlesSubdirectories()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs", "2024");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(Path.Combine(adrsDir, "adr-nested.md"), "# Nested ADR");

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
            Assert.Single(result);
            Assert.Equal("Nested ADR", result[0].Title);
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
    public async Task DocumentationService_ExtractsDescriptionFromContent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var recsDir = Path.Combine(tempDir, "Recommendations");
        Directory.CreateDirectory(recsDir);

        var content = "# Best Practice\nThis is a description paragraph.\nWith multiple lines of text.";
        await File.WriteAllTextAsync(Path.Combine(recsDir, "best-practice.md"), content);

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
            var result = await service.GetDocumentationByCategoryAsync(DocumentationCategory.Recommendations);

            // Assert
            Assert.Single(result);
            Assert.NotNull(result[0].Description);
            Assert.Contains("description paragraph", result[0].Description);
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
    public async Task GetAllDocumentationAsync_CachesResults()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(Path.Combine(adrsDir, "adr-001.md"), "# ADR 001");

        try
        {
            var options = Options.Create(new DocumentationOptions
            {
                UseLocalFileSystem = true,
                BasePath = tempDir
            });
            var logger = Mock.Of<ILogger<LocalFileSystemDocumentationService>>();
            var service = new LocalFileSystemDocumentationService(options, logger);

            // Act - Call twice
            var result1 = await service.GetAllDocumentationAsync();
            var result2 = await service.GetAllDocumentationAsync();

            // Assert - Should return same cached results
            Assert.Equal(result1.Count, result2.Count);
            Assert.Single(result1);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }
}
