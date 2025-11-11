using Wigo4it.CodingGuidelines.Core.Services;
using Xunit;

namespace Wigo4it.CodingGuidelines.Tests;

public class GuidelinesServiceTests
{
    private readonly GuidelinesService _service;

    public GuidelinesServiceTests()
    {
        _service = new GuidelinesService();
    }

    [Fact]
    public void GetAllCodingGuidelines_ShouldReturnGuidelines()
    {
        // Act
        var guidelines = _service.GetAllCodingGuidelines();

        // Assert
        Assert.NotNull(guidelines);
        Assert.NotEmpty(guidelines);
    }

    [Fact]
    public void GetCodingGuidelineById_WithValidId_ShouldReturnGuideline()
    {
        // Arrange
        var id = "CG001";

        // Act
        var guideline = _service.GetCodingGuidelineById(id);

        // Assert
        Assert.NotNull(guideline);
        Assert.Equal(id, guideline.Id);
    }

    [Fact]
    public void GetCodingGuidelineById_WithInvalidId_ShouldReturnNull()
    {
        // Arrange
        var id = "INVALID";

        // Act
        var guideline = _service.GetCodingGuidelineById(id);

        // Assert
        Assert.Null(guideline);
    }

    [Fact]
    public void GetCodingGuidelinesByCategory_ShouldReturnFilteredGuidelines()
    {
        // Arrange
        var category = "Naming";

        // Act
        var guidelines = _service.GetCodingGuidelinesByCategory(category);

        // Assert
        Assert.NotNull(guidelines);
        Assert.NotEmpty(guidelines);
        Assert.All(guidelines, g => Assert.Equal(category, g.Category));
    }

    [Fact]
    public void GetCodingGuidelinesByLanguage_ShouldReturnFilteredGuidelines()
    {
        // Arrange
        var language = "C#";

        // Act
        var guidelines = _service.GetCodingGuidelinesByLanguage(language);

        // Assert
        Assert.NotNull(guidelines);
        Assert.NotEmpty(guidelines);
        Assert.All(guidelines, g => Assert.Equal(language, g.Language));
    }

    [Fact]
    public void GetAllStyleGuides_ShouldReturnStyleGuides()
    {
        // Act
        var styleGuides = _service.GetAllStyleGuides();

        // Assert
        Assert.NotNull(styleGuides);
        Assert.NotEmpty(styleGuides);
    }

    [Fact]
    public void GetStyleGuideById_WithValidId_ShouldReturnStyleGuide()
    {
        // Arrange
        var id = "SG001";

        // Act
        var styleGuide = _service.GetStyleGuideById(id);

        // Assert
        Assert.NotNull(styleGuide);
        Assert.Equal(id, styleGuide.Id);
    }

    [Fact]
    public void GetStyleGuideByLanguage_WithValidLanguage_ShouldReturnStyleGuide()
    {
        // Arrange
        var language = "C#";

        // Act
        var styleGuide = _service.GetStyleGuideByLanguage(language);

        // Assert
        Assert.NotNull(styleGuide);
        Assert.Equal(language, styleGuide.Language);
    }

    [Fact]
    public void GetAllADRs_ShouldReturnADRs()
    {
        // Act
        var adrs = _service.GetAllADRs();

        // Assert
        Assert.NotNull(adrs);
        Assert.NotEmpty(adrs);
    }

    [Fact]
    public void GetADRById_WithValidId_ShouldReturnADR()
    {
        // Arrange
        var id = "ADR001";

        // Act
        var adr = _service.GetADRById(id);

        // Assert
        Assert.NotNull(adr);
        Assert.Equal(id, adr.Id);
    }

    [Fact]
    public void GetADRsByStatus_WithValidStatus_ShouldReturnFilteredADRs()
    {
        // Arrange
        var status = "Accepted";

        // Act
        var adrs = _service.GetADRsByStatus(status);

        // Assert
        Assert.NotNull(adrs);
        Assert.NotEmpty(adrs);
        Assert.All(adrs, a => Assert.Equal(status, a.Status));
    }
}
