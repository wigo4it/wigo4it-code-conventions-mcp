using Wigo4it.CodeGuidelines.Server.Configuration;

namespace Wigo4it.CodeGuidelines.Tests;

public class DocumentationOptionsTests
{
    [Fact]
    public void GetEffectiveBasePath_ReturnsBasePath_WhenSet()
    {
        // Arrange
        var options = new DocumentationOptions
        {
            BasePath = @"C:\custom\path"
        };

        // Act
        var result = options.GetEffectiveBasePath();

        // Assert
        Assert.Equal(@"C:\custom\path", result);
    }

    [Fact]
    public void GetEffectiveBasePath_ReturnsDefaultPath_WhenBasePathIsNull()
    {
        // Arrange
        var options = new DocumentationOptions
        {
            BasePath = null
        };

        // Act
        var result = options.GetEffectiveBasePath();

        // Assert
        Assert.NotNull(result);
        Assert.EndsWith("docs", result);
    }

    [Fact]
    public void UseLocalFileSystem_DefaultsToFalse()
    {
        // Arrange & Act
        var options = new DocumentationOptions();

        // Assert
        Assert.False(options.UseLocalFileSystem);
    }
}
