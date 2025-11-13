using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Moq.Protected;
using System.Net;
using System.Text.Json;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;

namespace Wigo4it.CodeGuidelines.Tests;

/// <summary>
/// Tests for the new search, related documents, and tag filtering functionality in GitHubDocumentationService.
/// These are unit tests using mocked HTTP responses.
/// </summary>
public class GitHubDocumentationServiceSearchTests
{
    private GitHubDocumentationService CreateServiceWithMockedHttpClient(
        Mock<HttpMessageHandler> handlerMock)
    {
        var httpClient = new HttpClient(handlerMock.Object)
        {
            BaseAddress = new Uri("https://api.github.com")
        };

        var httpClientFactory = new Mock<IHttpClientFactory>();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = false,
            GitHubOwner = "wigo4it",
            GitHubRepository = "wigo4it-code-conventions-mcp",
            GitHubBranch = "main",
            DocsPath = "docs"
        });

        var logger = Mock.Of<ILogger<GitHubDocumentationService>>();

        return new GitHubDocumentationService(options, httpClientFactory.Object, logger);
    }

    private void SetupMockHttpResponses(
        Mock<HttpMessageHandler> handlerMock,
        Dictionary<string, string> urlResponses)
    {
        foreach (var kvp in urlResponses)
        {
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.RequestUri != null &&
                        req.RequestUri.ToString().Contains(kvp.Key)),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(kvp.Value)
                });
        }
    }

    #region Search Tests

    [Fact]
    public async Task SearchDocumentationAsync_FindsMatchInTitle()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001-aspire.md", path = "docs/ADRs/adr-001-aspire.md", type = "file" }
        });

        var fileContent = "# ADR-001: Use Aspire\n\nThis document describes using Aspire.";

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse },
            { "raw.githubusercontent.com", fileContent }
        });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.SearchDocumentationAsync("aspire");

        // Assert
        Assert.Single(results);
        Assert.Contains("aspire", results[0].Metadata.Title, StringComparison.OrdinalIgnoreCase);
        Assert.True(results[0].RelevanceScore > 0);
    }

    [Fact]
    public async Task SearchDocumentationAsync_ReturnsEmptyForNoMatches()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" }
        });

        var fileContent = "# ADR-001: Architecture Decision\n\nThis is about databases.";

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse },
            { "raw.githubusercontent.com", fileContent }
        });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.SearchDocumentationAsync("kubernetes");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchDocumentationAsync_ReturnsEmptyForEmptySearchTerm()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateServiceWithMockedHttpClient(handlerMock);

        // Act
        var results = await service.SearchDocumentationAsync("");

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task SearchDocumentationAsync_RanksResultsByRelevance()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001-aspire.md", path = "docs/ADRs/adr-001-aspire.md", type = "file" },
            new { name = "adr-002.md", path = "docs/ADRs/adr-002.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse }
        });

        // Setup different content for different files
        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("adr-001-aspire.md")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("# Use Aspire for Development\n\nSome content here.")
            });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("adr-002.md") &&
                    !req.RequestUri.ToString().Contains("adr-002-")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("# Architecture Decision\n\nWe might use aspire in the future.")
            });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.SearchDocumentationAsync("aspire");

        // Assert
        Assert.Equal(2, results.Count);
        Assert.True(results[0].RelevanceScore >= results[1].RelevanceScore);
    }

    #endregion

    #region Related Documents Tests

    [Fact]
    public async Task GetRelatedDocumentationAsync_FindsDocumentsInSameCategory()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" },
            new { name = "adr-002.md", path = "docs/ADRs/adr-002.md", type = "file" },
            new { name = "adr-003.md", path = "docs/ADRs/adr-003.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse }
        });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("raw.githubusercontent.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                var uri = req.RequestUri!.ToString();
                string content;
                if (uri.Contains("adr-001.md"))
                    content = "# ADR-001: Use Aspire\n\nAspire is a development tool.";
                else if (uri.Contains("adr-002.md"))
                    content = "# ADR-002: Use Docker\n\nDocker is a container platform.";
                else
                    content = "# ADR-003: Development Tools\n\nWe use various development tools.";

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
            });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetRelatedDocumentationAsync("adrs/adr-001", 5);

        // Assert
        Assert.NotEmpty(results);
        Assert.DoesNotContain(results, d => d.Id == "adrs/adr-001");
        Assert.All(results, d => Assert.Equal(DocumentationCategory.ADRs, d.Category));
    }

    [Fact]
    public async Task GetRelatedDocumentationAsync_RespectsMaxResults()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var files = Enumerable.Range(1, 10).Select(i =>
            new { name = $"adr-{i:D3}.md", path = $"docs/ADRs/adr-{i:D3}.md", type = "file" }
        ).ToArray();

        var contentsResponse = JsonSerializer.Serialize(files);

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse }
        });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("raw.githubusercontent.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                var content = $"# ADR\n\nDocument content.";
                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
            });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetRelatedDocumentationAsync("adrs/adr-001", 3);

        // Assert
        Assert.Equal(3, results.Count);
    }

    [Fact]
    public async Task GetRelatedDocumentationAsync_ReturnsEmptyForNonExistentDocument()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse },
            { "raw.githubusercontent.com", "# ADR-001" }
        });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetRelatedDocumentationAsync("adrs/nonexistent", 5);

        // Assert
        Assert.Empty(results);
    }

    #endregion

    #region Tag Filtering Tests

    [Fact]
    public async Task GetDocumentationByTagsAsync_FindsDocumentsWithTags()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" },
            new { name = "adr-002.md", path = "docs/ADRs/adr-002.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse }
        });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("raw.githubusercontent.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                var uri = req.RequestUri!.ToString();
                string content;
                if (uri.Contains("adr-001.md"))
                    content = "---\ntags: [architecture, microservices]\n---\n# ADR-001";
                else
                    content = "---\ntags: [database, persistence]\n---\n# ADR-002";

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
            });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetDocumentationByTagsAsync(new[] { "architecture" });

        // Assert
        Assert.Single(results);
        Assert.Equal("adrs/adr-001", results[0].Id);
    }

    [Fact]
    public async Task GetDocumentationByTagsAsync_FindsDocumentsWithMultipleTags()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" },
            new { name = "adr-002.md", path = "docs/ADRs/adr-002.md", type = "file" },
            new { name = "adr-003.md", path = "docs/ADRs/adr-003.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse }
        });

        handlerMock
            .Protected()
            .Setup<Task<HttpResponseMessage>>(
                "SendAsync",
                ItExpr.Is<HttpRequestMessage>(req =>
                    req.RequestUri != null &&
                    req.RequestUri.ToString().Contains("raw.githubusercontent.com")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
            {
                var uri = req.RequestUri!.ToString();
                string content;
                if (uri.Contains("adr-001.md"))
                    content = "---\ntags: [architecture, microservices]\n---\n# ADR-001";
                else if (uri.Contains("adr-002.md"))
                    content = "---\ntags: [database, persistence]\n---\n# ADR-002";
                else
                    content = "---\ntags: [testing, quality]\n---\n# ADR-003";

                return new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(content)
                };
            });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetDocumentationByTagsAsync(new[] { "architecture", "database" });

        // Assert
        Assert.Equal(2, results.Count);
        Assert.Contains(results, d => d.Id == "adrs/adr-001");
        Assert.Contains(results, d => d.Id == "adrs/adr-002");
    }

    [Fact]
    public async Task GetDocumentationByTagsAsync_IsCaseInsensitive()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse },
            { "raw.githubusercontent.com", "---\ntags: [Architecture, Microservices]\n---\n# ADR-001" }
        });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetDocumentationByTagsAsync(new[] { "architecture" });

        // Assert
        Assert.Single(results);
    }

    [Fact]
    public async Task GetDocumentationByTagsAsync_ReturnsEmptyForNoMatches()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();

        var contentsResponse = JsonSerializer.Serialize(new[]
        {
            new { name = "adr-001.md", path = "docs/ADRs/adr-001.md", type = "file" }
        });

        SetupMockHttpResponses(handlerMock, new Dictionary<string, string>
        {
            { "/repos/wigo4it/wigo4it-code-conventions-mcp/contents/docs/ADRs", contentsResponse },
            { "raw.githubusercontent.com", "---\ntags: [architecture, microservices]\n---\n# ADR-001" }
        });

        var service = CreateServiceWithMockedHttpClient(handlerMock);
        await service.GetAllDocumentationAsync(); // Initialize

        // Act
        var results = await service.GetDocumentationByTagsAsync(new[] { "nonexistent" });

        // Assert
        Assert.Empty(results);
    }

    [Fact]
    public async Task GetDocumentationByTagsAsync_ReturnsEmptyForEmptyTagList()
    {
        // Arrange
        var handlerMock = new Mock<HttpMessageHandler>();
        var service = CreateServiceWithMockedHttpClient(handlerMock);

        // Act
        var results = await service.GetDocumentationByTagsAsync(Array.Empty<string>());

        // Assert
        Assert.Empty(results);
    }

    #endregion
}
