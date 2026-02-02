using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Records;
using FindThatBook.Domain.ValueObjects;
using FluentAssertions;

namespace FindThatBook.UnitTests.Domain;

public class BookMatchTests
{
    private Book CreateTestBook(string title = "Test Book", string author = "Test Author")
    {
        return new Book(
            title: title,
            primaryAuthor: new Author(author),
            openLibraryWorkId: "OL123W"
        );
    }

    [Fact]
    public void CreateStrongest_ShouldReturnStrongestMatch()
    {
        // Arrange
        var book = CreateTestBook("Cien años de soledad", "García Márquez");

        // Act
        var match = BookMatch.CreateStrongest(book, "García Márquez");

        // Assert
        match.Strength.Should().Be(MatchStrength.Strongest);
        match.Score.Should().Be(1.0);
        match.Explanation.Should().Contain("García Márquez");
    }

    [Fact]
    public void CreateStrong_ShouldReturnStrongMatch()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var match = BookMatch.CreateStrong(book, "Contributor Name", "contributor");

        // Assert
        match.Strength.Should().Be(MatchStrength.Strong);
        match.Score.Should().Be(0.8);
    }

    [Fact]
    public void CreateMedium_ShouldReturnMediumMatch()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var match = BookMatch.CreateMedium(book, "Similar Title", 0.7);

        // Assert
        match.Strength.Should().Be(MatchStrength.Medium);
        match.Score.Should().Be(0.7);
    }

    [Fact]
    public void CreateWeak_ShouldReturnWeakMatch()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var match = BookMatch.CreateWeak(book, "Author Name");

        // Assert
        match.Strength.Should().Be(MatchStrength.Weak);
        match.Score.Should().Be(0.5);
    }

    [Fact]
    public void CreateVeryWeak_ShouldReturnVeryWeakMatch()
    {
        // Arrange
        var book = CreateTestBook();
        var keywords = new List<string> { "fantasy", "magic" };

        // Act
        var match = BookMatch.CreateVeryWeak(book, keywords);

        // Assert
        match.Strength.Should().Be(MatchStrength.VeryWeak);
        match.Score.Should().Be(0.3);
        match.Explanation.Should().Contain("fantasy");
    }

    [Fact]
    public void CreateVeryWeak_WithEmptyKeywords_ShouldThrowArgumentException()
    {
        // Arrange
        var book = CreateTestBook();

        // Act
        var act = () => BookMatch.CreateVeryWeak(book, new List<string>());

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}