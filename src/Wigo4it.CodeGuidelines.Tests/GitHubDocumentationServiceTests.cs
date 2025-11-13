using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Wigo4it.CodeGuidelines.Server.Configuration;
using Wigo4it.CodeGuidelines.Server.Models;
using Wigo4it.CodeGuidelines.Server.Services;
using Xunit;

namespace Wigo4it.CodeGuidelines.Tests;

public class GitHubDocumentationServiceTests
{
    // Note: These are integration tests that require network access and may fail if the repository is private
    // To run these tests, ensure the repository is public or set a valid GitHub token in the configuration

    [Fact(Skip = "Integration test - requires network access and public repository")]
    public async Task GitHubDocumentationService_CanFetchFromPublicRepo()
    {
        // Arrange
        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = false,
            GitHubOwner = "wigo4it",
            GitHubRepository = "wigo4it-code-conventions-mcp",
            GitHubBranch = "main",
            DocsPath = "docs"
        });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = Mock.Of<ILogger<GitHubDocumentationService>>();
        var service = new GitHubDocumentationService(options, httpClientFactory.Object, logger);

        // Act
        var docs = await service.GetAllDocumentationAsync();

        // Assert
        Assert.NotNull(docs);
        Assert.NotEmpty(docs);
    }

    [Fact(Skip = "Integration test - requires network access and public repository")]
    public async Task GitHubDocumentationService_CanFilterByCategory()
    {
        // Arrange
        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = false,
            GitHubOwner = "wigo4it",
            GitHubRepository = "wigo4it-code-conventions-mcp",
            GitHubBranch = "main",
            DocsPath = "docs"
        });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = Mock.Of<ILogger<GitHubDocumentationService>>();
        var service = new GitHubDocumentationService(options, httpClientFactory.Object, logger);

        // Act
        var adrDocs = await service.GetDocumentationByCategoryAsync(DocumentationCategory.ADRs);
        var styleGuideDocs = await service.GetDocumentationByCategoryAsync(DocumentationCategory.StyleGuides);

        // Assert
        Assert.NotNull(adrDocs);
        Assert.NotEmpty(adrDocs);
        Assert.All(adrDocs, doc => Assert.Equal(DocumentationCategory.ADRs, doc.Category));

        Assert.NotNull(styleGuideDocs);
        Assert.NotEmpty(styleGuideDocs);
        Assert.All(styleGuideDocs, doc => Assert.Equal(DocumentationCategory.StyleGuides, doc.Category));
    }

    [Fact(Skip = "Integration test - requires network access and public repository")]
    public async Task GitHubDocumentationService_CanFetchContent()
    {
        // Arrange
        var options = Options.Create(new DocumentationOptions
        {
            UseLocalFileSystem = false,
            GitHubOwner = "wigo4it",
            GitHubRepository = "wigo4it-code-conventions-mcp",
            GitHubBranch = "main",
            DocsPath = "docs"
        });

        var httpClientFactory = new Mock<IHttpClientFactory>();
        var httpClient = new HttpClient();
        httpClientFactory.Setup(f => f.CreateClient(It.IsAny<string>())).Returns(httpClient);

        var logger = Mock.Of<ILogger<GitHubDocumentationService>>();
        var service = new GitHubDocumentationService(options, httpClientFactory.Object, logger);

        // Get first document
        var docs = await service.GetAllDocumentationAsync();
        var firstDoc = docs.FirstOrDefault();
        Assert.NotNull(firstDoc);

        // Act
        var content = await service.GetDocumentationContentAsync(firstDoc.Id);

        // Assert
        Assert.NotNull(content);
        Assert.NotNull(content.Content);
        Assert.NotEmpty(content.Content);
        Assert.Equal(firstDoc.Id, content.Metadata.Id);
    }
}
