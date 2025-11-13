using Wigo4it.CodeGuidelines.Server.Models;

namespace Wigo4it.CodeGuidelines.Tests;

public class ModelTests
{
    [Fact]
    public void DocumentationMetadata_CanBeCreated()
    {
        // Arrange & Act
        var metadata = new DocumentationMetadata
        {
            Id = "test/doc",
            Title = "Test Document",
            Category = DocumentationCategory.ADRs,
            FilePath = "/path/to/doc.md",
            Description = "Test description"
        };

        // Assert
        Assert.Equal("test/doc", metadata.Id);
        Assert.Equal("Test Document", metadata.Title);
        Assert.Equal(DocumentationCategory.ADRs, metadata.Category);
        Assert.Equal("/path/to/doc.md", metadata.FilePath);
        Assert.Equal("Test description", metadata.Description);
    }

    [Fact]
    public void DocumentationContent_CanBeCreated()
    {
        // Arrange
        var metadata = new DocumentationMetadata
        {
            Id = "test/doc",
            Title = "Test",
            Category = DocumentationCategory.Recommendations,
            FilePath = "/test.md"
        };

        // Act
        var content = new DocumentationContent
        {
            Metadata = metadata,
            Content = "# Test\n\nContent here"
        };

        // Assert
        Assert.NotNull(content.Metadata);
        Assert.Equal("# Test\n\nContent here", content.Content);
    }

    [Fact]
    public void DocumentationCategory_HasCorrectValues()
    {
        // Arrange & Act & Assert
        Assert.Equal(4, Enum.GetValues<DocumentationCategory>().Length);
        Assert.True(Enum.IsDefined(typeof(DocumentationCategory), DocumentationCategory.ADRs));
        Assert.True(Enum.IsDefined(typeof(DocumentationCategory), DocumentationCategory.Recommendations));
        Assert.True(Enum.IsDefined(typeof(DocumentationCategory), DocumentationCategory.StyleGuides));
        Assert.True(Enum.IsDefined(typeof(DocumentationCategory), DocumentationCategory.Structures));
    }
}
