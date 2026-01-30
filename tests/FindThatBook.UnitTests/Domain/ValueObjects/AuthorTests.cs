using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.UnitTests.Domain.ValueObjects;

public class AuthorTests
{
    [Fact]
    public void Constructor_WithValidName_CreatesAuthor()
    {
        // Arrange & Act
        var author = new Author("J.R.R. Tolkien");

        // Assert
        Assert.Equal("J.R.R. Tolkien", author.Name);
        Assert.Null(author.OpenLibraryId);
    }

    [Fact]
    public void Constructor_WithNameAndId_CreatesAuthor()
    {
        // Arrange & Act
        var author = new Author("J.R.R. Tolkien", "/authors/OL26320A");

        // Assert
        Assert.Equal("J.R.R. Tolkien", author.Name);
        Assert.Equal("/authors/OL26320A", author.OpenLibraryId);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   \t  \n  ")]
    public void Constructor_WithEmptyName_ThrowsArgumentException(string emptyName)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new Author(emptyName));

        Assert.Equal("name", exception.ParamName);
        Assert.Contains("nombre del autor  no puede estar vacío", exception.Message);
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange & Act
        var author = new Author("  J.R.R. Tolkien  ");

        // Assert
        Assert.Equal("J.R.R. Tolkien", author.Name);
    }

    [Fact]
    public void GetNormalizedName_ReturnsLowercaseAndTrimmed()
    {
        // Arrange
        var author = new Author("  J.R.R. TOLKIEN  ");

        // Act
        var normalized = author.GetNormalizedName();

        // Assert
        Assert.Equal("j.r.r. tolkien", normalized);
    }

    [Fact]
    public void GetNormalizedName_WithSpecialCharacters_PreservesSpecialCharacters()
    {
        // Arrange
        var author = new Author("Joséf Škvorecký");

        // Act
        var normalized = author.GetNormalizedName();

        // Assert
        Assert.Equal("joséf škvorecký", normalized);
    }

    [Fact]
    public void ToString_ReturnsName()
    {
        // Arrange
        var author = new Author("J.R.R. Tolkien");

        // Act
        var result = author.ToString();

        // Assert
        Assert.Equal("J.R.R. Tolkien", result);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var author1 = new Author("J.R.R. Tolkien", "/authors/OL26320A");
        var author2 = new Author("J.R.R. Tolkien", "/authors/OL26320A");

        // Act & Assert
        Assert.Equal(author1, author2);
        Assert.True(author1 == author2);
        Assert.False(author1 != author2);
    }

    [Fact]
    public void RecordEquality_DifferentNames_AreNotEqual()
    {
        // Arrange
        var author1 = new Author("J.R.R. Tolkien");
        var author2 = new Author("George R.R. Martin");

        // Act & Assert
        Assert.NotEqual(author1, author2);
        Assert.False(author1 == author2);
        Assert.True(author1 != author2);
    }

    [Fact]
    public void RecordEquality_DifferentIds_AreNotEqual()
    {
        // Arrange
        var author1 = new Author("J.R.R. Tolkien", "/authors/OL26320A");
        var author2 = new Author("J.R.R. Tolkien", "/authors/OL99999A");

        // Act & Assert
        Assert.NotEqual(author1, author2);
    }

    [Fact]
    public void RecordEquality_OneWithIdOneWithout_AreNotEqual()
    {
        // Arrange
        var author1 = new Author("J.R.R. Tolkien", "/authors/OL26320A");
        var author2 = new Author("J.R.R. Tolkien");

        // Act & Assert
        Assert.NotEqual(author1, author2);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var author1 = new Author("J.R.R. Tolkien", "/authors/OL26320A");
        var author2 = new Author("J.R.R. Tolkien", "/authors/OL26320A");

        // Act & Assert
        Assert.Equal(author1.GetHashCode(), author2.GetHashCode());
    }

    [Fact]
    public void Constructor_WithNullId_AcceptsNull()
    {
        // Arrange & Act
        var author = new Author("J.R.R. Tolkien", null);

        // Assert
        Assert.Equal("J.R.R. Tolkien", author.Name);
        Assert.Null(author.OpenLibraryId);
    }

    [Fact]
    public void GetNormalizedName_WithMultipleSpaces_PreservesSpaces()
    {
        // Arrange
        var author = new Author("Jean Paul Sartre");

        // Act
        var normalized = author.GetNormalizedName();

        // Assert
        Assert.Equal("jean paul sartre", normalized);
    }

    [Fact]
    public void Constructor_WithSingleCharacterName_CreatesAuthor()
    {
        // Arrange & Act
        var author = new Author("X");

        // Assert
        Assert.Equal("X", author.Name);
    }

    [Fact]
    public void Constructor_WithVeryLongName_CreatesAuthor()
    {
        // Arrange
        var longName = new string('A', 1000);

        // Act
        var author = new Author(longName);

        // Assert
        Assert.Equal(longName, author.Name);
    }

    [Fact]
    public void RecordEquality_CaseInsensitiveNames_AreNotEqual()
    {
        // Arrange
        var author1 = new Author("tolkien");
        var author2 = new Author("TOLKIEN");

        // Act & Assert
        Assert.NotEqual(author1, author2);
    }

    [Fact]
    public void GetNormalizedName_IsIdempotent()
    {
        // Arrange
        var author = new Author("J.R.R. Tolkien");

        // Act
        var normalized1 = author.GetNormalizedName();
        var normalized2 = author.GetNormalizedName();

        // Assert
        Assert.Equal(normalized1, normalized2);
    }
}