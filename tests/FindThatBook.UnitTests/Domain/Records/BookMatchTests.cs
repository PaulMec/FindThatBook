using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Records;
using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.UnitTests.Domain.Records;

public class BookMatchTests
{
    private readonly Book _testBook;
    private readonly Author _testAuthor;

    public BookMatchTests()
    {
        _testAuthor = new Author("J.R.R. Tolkien", "/authors/OL26320A");
        _testBook = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: "/works/OL27516W",
            firstPublishYear: 1937
        );
    }

    [Fact]
    public void Constructor_WithValidParameters_CreatesBookMatch()
    {
        // Arrange & Act
        var match = new BookMatch(
            book: _testBook,
            strength: MatchStrength.Strongest,
            explanation: "Perfect match",
            score: 1.0
        );

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.Strongest, match.Strength);
        Assert.Equal("Perfect match", match.Explanation);
        Assert.Equal(1.0, match.Score);
    }

    [Fact]
    public void Constructor_WithNullBook_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new BookMatch(null!, MatchStrength.Strongest, "Explanation", 1.0)
        );

        Assert.Equal("book", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyExplanation_ThrowsArgumentException(string emptyExplanation)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new BookMatch(_testBook, MatchStrength.Strongest, emptyExplanation, 1.0)
        );

        Assert.Equal("explanation", exception.ParamName);
        Assert.Contains("explicación no puede estar vacía", exception.Message);
    }

    [Fact]
    public void Constructor_TrimsExplanation()
    {
        // Arrange & Act
        var match = new BookMatch(
            book: _testBook,
            strength: MatchStrength.Strongest,
            explanation: "  Perfect match  ",
            score: 1.0
        );

        // Assert
        Assert.Equal("Perfect match", match.Explanation);
    }

    [Fact]
    public void Constructor_WithDefaultScore_SetsScoreToZero()
    {
        // Arrange & Act
        var match = new BookMatch(
            book: _testBook,
            strength: MatchStrength.Strongest,
            explanation: "Perfect match"
        );

        // Assert
        Assert.Equal(0, match.Score);
    }

    [Fact]
    public void CreateStrongest_CreatesMatchWithCorrectProperties()
    {
        // Arrange & Act
        var match = BookMatch.CreateStrongest(_testBook, "Tolkien");

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.Strongest, match.Strength);
        Assert.Contains("Coincidencia exacta del título", match.Explanation);
        Assert.Contains("Tolkien es el autor principal", match.Explanation);
        Assert.Equal(1.0, match.Score);
    }

    [Fact]
    public void CreateStrong_CreatesMatchWithCorrectProperties()
    {
        // Arrange & Act
        var match = BookMatch.CreateStrong(_testBook, "Tolkien", "ilustrador");

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.Strong, match.Strength);
        Assert.Contains("Coincidencia exacta del título", match.Explanation);
        Assert.Contains("Tolkien aparece como ilustrador", match.Explanation);
        Assert.Equal(0.8, match.Score);
    }

    [Fact]
    public void CreateMedium_CreatesMatchWithCorrectProperties()
    {
        // Arrange & Act
        var match = BookMatch.CreateMedium(_testBook, "The Hobbit", 0.75);

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.Medium, match.Strength);
        Assert.Contains("Título similar", match.Explanation);
        Assert.Contains("The Hobbit", match.Explanation);
        Assert.Contains("75 %", match.Explanation);
        Assert.Equal(0.75, match.Score);
    }

    [Fact]
    public void CreateWeak_CreatesMatchWithCorrectProperties()
    {
        // Arrange & Act
        var match = BookMatch.CreateWeak(_testBook, "Tolkien");

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.Weak, match.Strength);
        Assert.Contains("Sin título", match.Explanation);
        Assert.Contains("mejores obras de Tolkien", match.Explanation);
        Assert.Equal(0.5, match.Score);
    }

    [Fact]
    public void CreateVeryWeak_WithSingleKeyword_CreatesMatchWithCorrectProperties()
    {
        // Arrange
        var keywords = new[] { "fantasy" };

        // Act
        var match = BookMatch.CreateVeryWeak(_testBook, keywords);

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.VeryWeak, match.Strength);
        Assert.Contains("Coincidencia de palabras clave", match.Explanation);
        Assert.Contains("fantasy", match.Explanation);
        Assert.Equal(0.3, match.Score);
    }

    [Fact]
    public void CreateVeryWeak_WithMultipleKeywords_CreatesMatchWithCorrectProperties()
    {
        // Arrange
        var keywords = new[] { "fantasy", "adventure", "dragon" };

        // Act
        var match = BookMatch.CreateVeryWeak(_testBook, keywords);

        // Assert
        Assert.Equal(_testBook, match.Book);
        Assert.Equal(MatchStrength.VeryWeak, match.Strength);
        Assert.Contains("fantasy, adventure, dragon", match.Explanation);
        Assert.Equal(0.3, match.Score);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var match1 = BookMatch.CreateStrongest(_testBook, "Tolkien");
        var match2 = BookMatch.CreateStrongest(_testBook, "Tolkien");

        // Act & Assert
        Assert.Equal(match1, match2);
    }

    [Fact]
    public void RecordEquality_DifferentBooks_AreNotEqual()
    {
        // Arrange
        var book2 = new Book(
            title: "The Lord of the Rings",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: "/works/OL27448W"
        );
        var match1 = BookMatch.CreateStrongest(_testBook, "Tolkien");
        var match2 = BookMatch.CreateStrongest(book2, "Tolkien");

        // Act & Assert
        Assert.NotEqual(match1, match2);
    }

    [Fact]
    public void RecordEquality_DifferentStrengths_AreNotEqual()
    {
        // Arrange
        var match1 = new BookMatch(_testBook, MatchStrength.Strongest, "Explanation", 1.0);
        var match2 = new BookMatch(_testBook, MatchStrength.Strong, "Explanation", 1.0);

        // Act & Assert
        Assert.NotEqual(match1, match2);
    }

    [Fact]
    public void CreateMedium_WithZeroSimilarity_CreatesMatch()
    {
        // Arrange & Act
        var match = BookMatch.CreateMedium(_testBook, "The Hobbit", 0.0);

        // Assert
        Assert.Equal(0.0, match.Score);
        Assert.Contains("0 %", match.Explanation);
    }

    [Fact]
    public void CreateMedium_WithFullSimilarity_CreatesMatch()
    {
        // Arrange & Act
        var match = BookMatch.CreateMedium(_testBook, "The Hobbit", 1.0);

        // Assert
        Assert.Equal(1.0, match.Score);
        Assert.Contains("100 %", match.Explanation);
    }

    [Fact]
    public void CreateVeryWeak_WithEmptyKeywords_CreatesMatch()
    {
        // Arrange
        var keywords = Array.Empty<string>();

        // Act
        var match = BookMatch.CreateVeryWeak(_testBook, keywords);

        // Assert
        Assert.Equal(MatchStrength.VeryWeak, match.Strength);
        Assert.NotEmpty(match.Explanation);
    }

    [Fact]
    public void Constructor_WithNegativeScore_AcceptsValue()
    {
        // Arrange & Act
        var match = new BookMatch(_testBook, MatchStrength.VeryWeak, "Low confidence", -0.5);

        // Assert
        Assert.Equal(-0.5, match.Score);
    }

    [Fact]
    public void Constructor_WithScoreAboveOne_AcceptsValue()
    {
        // Arrange & Act
        var match = new BookMatch(_testBook, MatchStrength.Strongest, "Extra high confidence", 1.5);

        // Assert
        Assert.Equal(1.5, match.Score);
    }

    [Fact]
    public void CreateStrong_WithDifferentRoles_CreatesCorrectExplanations()
    {
        // Arrange & Act
        var match1 = BookMatch.CreateStrong(_testBook, "Tolkien", "editor");
        var match2 = BookMatch.CreateStrong(_testBook, "Lee", "ilustrador");

        // Assert
        Assert.Contains("editor", match1.Explanation);
        Assert.Contains("ilustrador", match2.Explanation);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var match1 = BookMatch.CreateStrongest(_testBook, "Tolkien");
        var match2 = BookMatch.CreateStrongest(_testBook, "Tolkien");

        // Act & Assert
        Assert.Equal(match1.GetHashCode(), match2.GetHashCode());
    }

    [Fact]
    public void AllMatchStrengths_CanBeUsedInBookMatch()
    {
        // Arrange & Act
        var strongest = new BookMatch(_testBook, MatchStrength.Strongest, "Test", 1.0);
        var strong = new BookMatch(_testBook, MatchStrength.Strong, "Test", 0.8);
        var medium = new BookMatch(_testBook, MatchStrength.Medium, "Test", 0.6);
        var weak = new BookMatch(_testBook, MatchStrength.Weak, "Test", 0.4);
        var veryWeak = new BookMatch(_testBook, MatchStrength.VeryWeak, "Test", 0.2);

        // Assert
        Assert.Equal(MatchStrength.Strongest, strongest.Strength);
        Assert.Equal(MatchStrength.Strong, strong.Strength);
        Assert.Equal(MatchStrength.Medium, medium.Strength);
        Assert.Equal(MatchStrength.Weak, weak.Strength);
        Assert.Equal(MatchStrength.VeryWeak, veryWeak.Strength);
    }

    [Fact]
    public void CreateMedium_WithDecimalSimilarity_FormatsCorrectly()
    {
        // Arrange & Act
        var match = BookMatch.CreateMedium(_testBook, "The Hobbit", 0.856);

        // Assert
        Assert.Contains("86 %", match.Explanation);
    }

    [Fact]
    public void CreateVeryWeak_PreservesKeywordOrder()
    {
        // Arrange
        var keywords = new[] { "alpha", "beta", "gamma" };

        // Act
        var match = BookMatch.CreateVeryWeak(_testBook, keywords);

        // Assert
        Assert.Contains("alpha, beta, gamma", match.Explanation);
    }
}