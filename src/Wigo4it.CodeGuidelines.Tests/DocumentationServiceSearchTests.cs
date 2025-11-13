using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;

namespace Wigo4it.CodeGuidelines.Tests;

/// <summary>
/// Tests for the new search, related documents, and tag filtering functionality.
/// </summary>
public class DocumentationServiceSearchTests
{
    #region Search Tests

    [Fact]
    public async Task SearchDocumentationAsync_FindsMatchInTitle()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001-aspire.md"),
            "# ADR-001: Use Aspire\n\nThis document describes using Aspire.");

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
            var results = await service.SearchDocumentationAsync("aspire");

            // Assert
            Assert.Single(results);
            Assert.Contains("aspire", results[0].Metadata.Title, StringComparison.OrdinalIgnoreCase);
            Assert.True(results[0].RelevanceScore > 0);
            Assert.True(results[0].MatchCount > 0);
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
    public async Task SearchDocumentationAsync_FindsMatchInContent()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Architecture Decision\n\nWe decided to use microservices for scalability.");

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
            var results = await service.SearchDocumentationAsync("microservices");

            // Assert
            Assert.Single(results);
            Assert.True(results[0].MatchCount > 0);
            Assert.NotEmpty(results[0].MatchingExcerpts);
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
    public async Task SearchDocumentationAsync_ReturnsEmptyForNoMatches()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Architecture Decision\n\nThis is about databases.");

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
            var results = await service.SearchDocumentationAsync("kubernetes");

            // Assert
            Assert.Empty(results);
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
    public async Task SearchDocumentationAsync_ReturnsEmptyForEmptySearchTerm()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Architecture Decision");

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
            var results = await service.SearchDocumentationAsync("");

            // Assert
            Assert.Empty(results);
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
    public async Task SearchDocumentationAsync_RanksResultsByRelevance()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        // Doc with match in title (high relevance)
        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001-aspire.md"),
            "# Use Aspire for Development\n\nSome content here.");

        // Doc with match only in content (lower relevance)
        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "# Architecture Decision\n\nWe might use aspire in the future.");

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
            var results = await service.SearchDocumentationAsync("aspire");

            // Assert
            Assert.Equal(2, results.Count);
            // First result should have higher relevance score (title match)
            Assert.True(results[0].RelevanceScore > results[1].RelevanceScore);
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
    public async Task SearchDocumentationAsync_LimitsExcerpts()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        var content = "# ADR-001\n\n" +
                      "First line with keyword.\n" +
                      "Second line with keyword.\n" +
                      "Third line with keyword.\n" +
                      "Fourth line with keyword.\n" +
                      "Fifth line with keyword.\n";

        await File.WriteAllTextAsync(Path.Combine(adrsDir, "adr-001.md"), content);

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
            var results = await service.SearchDocumentationAsync("keyword");

            // Assert
            Assert.Single(results);
            // Should limit to 3 excerpts even though there are more matches
            Assert.True(results[0].MatchingExcerpts.Count <= 3);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    #endregion

    #region Related Documents Tests

    [Fact]
    public async Task GetRelatedDocumentationAsync_FindsDocumentsInSameCategory()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Use Aspire\n\nAspire is a development tool.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "# ADR-002: Use Docker\n\nDocker is a container platform.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-003.md"),
            "# ADR-003: Development Tools\n\nWe use various development tools.");

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
            var results = await service.GetRelatedDocumentationAsync("adrs/adr-001", 5);

            // Assert
            Assert.NotEmpty(results);
            Assert.DoesNotContain(results, d => d.Id == "adrs/adr-001"); // Should not include source doc
            Assert.All(results, d => Assert.Equal(DocumentationCategory.ADRs, d.Category));
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
    public async Task GetRelatedDocumentationAsync_RespectsMaxResults()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001\n\nBase document.");

        for (int i = 2; i <= 10; i++)
        {
            await File.WriteAllTextAsync(
                Path.Combine(adrsDir, $"adr-{i:D3}.md"),
                $"# ADR-{i:D3}\n\nRelated document {i}.");
        }

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
            var results = await service.GetRelatedDocumentationAsync("adrs/adr-001", 3);

            // Assert
            Assert.Equal(3, results.Count);
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
    public async Task GetRelatedDocumentationAsync_ReturnsEmptyForNonExistentDocument()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001");

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
            var results = await service.GetRelatedDocumentationAsync("adrs/nonexistent", 5);

            // Assert
            Assert.Empty(results);
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
    public async Task GetRelatedDocumentationAsync_FindsDocumentsWithSharedKeywords()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001: Microservices Architecture\n\nWe use microservices with containers and orchestration.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "# ADR-002: Container Platform\n\nContainers and orchestration are key technologies.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-003.md"),
            "# ADR-003: Database Choice\n\nWe selected PostgreSQL for persistence.");

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
            var results = await service.GetRelatedDocumentationAsync("adrs/adr-001", 5);

            // Assert
            Assert.NotEmpty(results);
            // ADR-002 should be more related than ADR-003 due to shared keywords
            if (results.Count > 1)
            {
                Assert.Equal("adrs/adr-002", results[0].Id);
            }
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    #endregion

    #region Tag Filtering Tests

    [Fact]
    public async Task GetDocumentationByTagsAsync_FindsDocumentsWithTags()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [architecture, microservices]\n---\n# ADR-001");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "---\ntags: [database, persistence]\n---\n# ADR-002");

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
            var results = await service.GetDocumentationByTagsAsync(new[] { "architecture" });

            // Assert
            Assert.Single(results);
            Assert.Equal("adrs/adr-001", results[0].Id);
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
    public async Task GetDocumentationByTagsAsync_FindsDocumentsWithMultipleTags()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [architecture, microservices]\n---\n# ADR-001");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "---\ntags: [database, persistence]\n---\n# ADR-002");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-003.md"),
            "---\ntags: [testing, quality]\n---\n# ADR-003");

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
            var results = await service.GetDocumentationByTagsAsync(new[] { "architecture", "database" });

            // Assert
            Assert.Equal(2, results.Count);
            Assert.Contains(results, d => d.Id == "adrs/adr-001");
            Assert.Contains(results, d => d.Id == "adrs/adr-002");
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
    public async Task GetDocumentationByTagsAsync_IsCaseInsensitive()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [Architecture, Microservices]\n---\n# ADR-001");

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
            var results = await service.GetDocumentationByTagsAsync(new[] { "architecture" });

            // Assert
            Assert.Single(results);
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
    public async Task GetDocumentationByTagsAsync_ReturnsEmptyForNoMatches()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [architecture, microservices]\n---\n# ADR-001");

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
            var results = await service.GetDocumentationByTagsAsync(new[] { "nonexistent" });

            // Assert
            Assert.Empty(results);
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
    public async Task GetDocumentationByTagsAsync_ReturnsEmptyForEmptyTagList()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "---\ntags: [architecture]\n---\n# ADR-001");

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
            var results = await service.GetDocumentationByTagsAsync(Array.Empty<string>());

            // Assert
            Assert.Empty(results);
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
    public async Task GetDocumentationByTagsAsync_HandlesDocumentsWithoutTags()
    {
        // Arrange
        var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        var adrsDir = Path.Combine(tempDir, "ADRs");
        Directory.CreateDirectory(adrsDir);

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-001.md"),
            "# ADR-001\n\nNo tags in this document.");

        await File.WriteAllTextAsync(
            Path.Combine(adrsDir, "adr-002.md"),
            "---\ntags: [architecture]\n---\n# ADR-002");

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
            var results = await service.GetDocumentationByTagsAsync(new[] { "architecture" });

            // Assert
            Assert.Single(results);
            Assert.Equal("adrs/adr-002", results[0].Id);
        }
        finally
        {
            if (Directory.Exists(tempDir))
            {
                Directory.Delete(tempDir, true);
            }
        }
    }

    #endregion
}
