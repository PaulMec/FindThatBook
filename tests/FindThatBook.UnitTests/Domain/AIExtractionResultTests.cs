using FindThatBook.Application.DTOs;
using FluentAssertions;

namespace FindThatBook.UnitTests.Domain;

public class AIExtractionResultTests
{
    [Fact]
    public void AIExtractionResult_ShouldCreateWithAllProperties()
    {
        // Arrange & Act
        var result = new AIExtractionResult
        {
            Title = "The Hobbit",
            Author = "Tolkien",
            Year = 1937,
            Keywords = new List<string> { "fantasy", "adventure" }
        };

        // Assert
        result.Title.Should().Be("The Hobbit");
        result.Author.Should().Be("Tolkien");
        result.Year.Should().Be(1937);
        result.Keywords.Should().HaveCount(2);
    }

    [Fact]
    public void AIExtractionResult_HasTitle_WhenTitleExists_ShouldReturnTrue()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = "Some Title",
            Author = null,
            Keywords = new List<string>()
        };

        // Act & Assert
        result.HasTitle.Should().BeTrue();
    }

    [Fact]
    public void AIExtractionResult_HasTitle_WhenTitleIsNull_ShouldReturnFalse()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = null,
            Author = "Author",
            Keywords = new List<string>()
        };

        // Act & Assert
        result.HasTitle.Should().BeFalse();
    }

    [Fact]
    public void AIExtractionResult_HasAuthor_WhenAuthorExists_ShouldReturnTrue()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = null,
            Author = "García Márquez",
            Keywords = new List<string>()
        };

        // Act & Assert
        result.HasAuthor.Should().BeTrue();
    }

    [Fact]
    public void AIExtractionResult_HasAuthor_WhenAuthorIsNull_ShouldReturnFalse()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = null,
            Author = null,
            Keywords = new List<string>()
        };

        // Act & Assert
        result.HasAuthor.Should().BeFalse();
    }

    [Fact]
    public void AIExtractionResult_HasAnyField_WhenKeywordsExist_ShouldReturnTrue()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = null,
            Author = null,
            Keywords = new List<string> { "magic", "school" }
        };

        // Act & Assert
        result.HasAnyField.Should().BeTrue();
    }

    [Fact]
    public void AIExtractionResult_HasAnyField_WhenEmpty_ShouldReturnFalse()
    {
        // Arrange
        var result = new AIExtractionResult
        {
            Title = null,
            Author = null,
            Keywords = new List<string>()
        };

        // Act & Assert
        result.HasAnyField.Should().BeFalse();
    }
}