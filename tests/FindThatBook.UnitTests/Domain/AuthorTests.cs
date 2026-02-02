using FindThatBook.Domain.ValueObjects;
using FluentAssertions;

namespace FindThatBook.UnitTests.Domain;

public class AuthorTests
{
    [Fact]
    public void Author_ShouldCreateWithValidName()
    {
        // Arrange & Act
        var author = new Author("Gabriel García Márquez");

        // Assert
        author.Name.Should().Be("Gabriel García Márquez");
    }

    [Fact]
    public void Author_ShouldCreateWithOpenLibraryId()
    {
        // Arrange & Act
        var author = new Author("Tolkien", "OL123A");

        // Assert
        author.Name.Should().Be("Tolkien");
        author.OpenLibraryId.Should().Be("OL123A");
    }

    [Fact]
    public void Author_GetNormalizedName_ShouldReturnLowerCase()
    {
        // Arrange
        var author = new Author("J.R.R. TOLKIEN");

        // Act
        var normalized = author.GetNormalizedName();

        // Assert
        normalized.Should().Be("j.r.r. tolkien");
    }

    [Fact]
    public void Author_WithEmptyName_ShouldThrowArgumentException()
    {
        // Act
        var act = () => new Author("");

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Author_ToString_ShouldReturnName()
    {
        // Arrange
        var author = new Author("Stephen King");

        // Act
        var result = author.ToString();

        // Assert
        result.Should().Be("Stephen King");
    }
}