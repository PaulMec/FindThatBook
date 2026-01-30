using FindThatBook.Domain.Entities;
using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.UnitTests.Domain.Entities;

public class BookTests
{
    private readonly Author _testAuthor = new Author("J.R.R. Tolkien", "/authors/OL26320A");
    private const string TestWorkId = "/works/OL27516W";

    [Fact]
    public void Constructor_WithValidParameters_CreatesBook()
    {
        // Arrange & Act
        var book = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            firstPublishYear: 1937,
            coverUrl: "https://covers.openlibrary.org/b/id/12345-L.jpg",
            description: "A classic fantasy novel"
        );

        // Assert
        Assert.Equal("The Hobbit", book.Title);
        Assert.Equal(_testAuthor, book.PrimaryAuthor);
        Assert.Equal(TestWorkId, book.OpenLibraryWorkId);
        Assert.Equal(1937, book.FirstPublishYear);
        Assert.Equal("https://covers.openlibrary.org/b/id/12345-L.jpg", book.CoverUrl);
        Assert.Equal("A classic fantasy novel", book.Description);
    }

    [Fact]
    public void Constructor_WithMinimalParameters_CreatesBook()
    {
        // Arrange & Act
        var book = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId
        );

        // Assert
        Assert.Equal("The Hobbit", book.Title);
        Assert.Equal(_testAuthor, book.PrimaryAuthor);
        Assert.Equal(TestWorkId, book.OpenLibraryWorkId);
        Assert.Null(book.FirstPublishYear);
        Assert.Null(book.CoverUrl);
        Assert.Null(book.Description);
        Assert.Empty(book.Contributors);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    public void Constructor_WithEmptyTitle_ThrowsArgumentException(string emptyTitle)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Book(emptyTitle, _testAuthor, TestWorkId)
        );

        Assert.Equal("title", exception.ParamName);
        Assert.Contains("título del libro no puede estar vacío", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    public void Constructor_WithEmptyWorkId_ThrowsArgumentException(string emptyWorkId)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() =>
            new Book("The Hobbit", _testAuthor, emptyWorkId)
        );

        Assert.Equal("openLibraryWorkId", exception.ParamName);
        Assert.Contains("ID de trabajo de OpenLibrary", exception.Message);
    }

    [Fact]
    public void Constructor_WithNullAuthor_ThrowsArgumentNullException()
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentNullException>(() =>
            new Book("The Hobbit", null!, TestWorkId)
        );

        Assert.Equal("primaryAuthor", exception.ParamName);
    }

    [Fact]
    public void Constructor_TrimsTitle()
    {
        // Arrange & Act
        var book = new Book("  The Hobbit  ", _testAuthor, TestWorkId);

        // Assert
        Assert.Equal("The Hobbit", book.Title);
    }

    [Fact]
    public void Constructor_WithContributors_StoresContributors()
    {
        // Arrange
        var contributors = new List<Author>
        {
            new Author("Christopher Tolkien"),
            new Author("Alan Lee")
        };

        // Act
        var book = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            contributors: contributors
        );

        // Assert
        Assert.Equal(2, book.Contributors.Count);
        Assert.Contains(book.Contributors, c => c.Name == "Christopher Tolkien");
        Assert.Contains(book.Contributors, c => c.Name == "Alan Lee");
    }

    [Fact]
    public void GetOpenLibraryUrl_ReturnsCorrectUrl()
    {
        // Arrange
        var book = new Book("The Hobbit", _testAuthor, TestWorkId);

        // Act
        var url = book.GetOpenLibraryUrl();

        // Assert
        Assert.Equal("https://openlibrary.org/works/OL27516W", url);
    }

    [Fact]
    public void HasAuthor_WithPrimaryAuthorName_ReturnsTrue()
    {
        // Arrange
        var book = new Book("The Hobbit", _testAuthor, TestWorkId);

        // Act & Assert
        Assert.True(book.HasAuthor("Tolkien"));
        Assert.True(book.HasAuthor("J.R.R. Tolkien"));
        Assert.True(book.HasAuthor("tolkien")); // Case insensitive
        Assert.True(book.HasAuthor("  Tolkien  ")); // Trimming
    }

    [Fact]
    public void HasAuthor_WithContributorName_ReturnsTrue()
    {
        // Arrange
        var contributors = new List<Author>
        {
            new Author("Christopher Tolkien"),
            new Author("Alan Lee")
        };
        var book = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            contributors: contributors
        );

        // Act & Assert
        Assert.True(book.HasAuthor("Christopher Tolkien"));
        Assert.True(book.HasAuthor("christopher"));
        Assert.True(book.HasAuthor("Alan Lee"));
        Assert.True(book.HasAuthor("lee"));
    }

    [Fact]
    public void HasAuthor_WithNonexistentAuthor_ReturnsFalse()
    {
        // Arrange
        var book = new Book("The Hobbit", _testAuthor, TestWorkId);

        // Act & Assert
        Assert.False(book.HasAuthor("George R.R. Martin"));
        Assert.False(book.HasAuthor("Unknown Author"));
    }

    [Fact]
    public void HasAuthor_WithPartialName_ReturnsTrue()
    {
        // Arrange
        var book = new Book("The Hobbit", _testAuthor, TestWorkId);

        // Act & Assert
        Assert.True(book.HasAuthor("Tolkien"));
        Assert.True(book.HasAuthor("R.R."));
    }

    [Fact]
    public void GetNormalizedTitle_ReturnsLowercaseAndTrimmedTitle()
    {
        // Arrange
        var book = new Book("  The HOBBIT  ", _testAuthor, TestWorkId);

        // Act
        var normalized = book.GetNormalizedTitle();

        // Assert
        Assert.Equal("the hobbit", normalized);
    }

    [Fact]
    public void Constructor_WithNegativeYear_AcceptsValue()
    {
        // Arrange & Act
        var book = new Book(
            title: "Ancient Text",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            firstPublishYear: -500
        );

        // Assert
        Assert.Equal(-500, book.FirstPublishYear);
    }

    [Fact]
    public void Constructor_WithFutureYear_AcceptsValue()
    {
        // Arrange & Act
        var book = new Book(
            title: "Future Book",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            firstPublishYear: 2050
        );

        // Assert
        Assert.Equal(2050, book.FirstPublishYear);
    }

    [Fact]
    public void HasAuthor_WithEmptyString_ReturnsFalse()
    {
        // Arrange
        var book = new Book("The Hobbit", _testAuthor, TestWorkId);

        // Act & Assert
        Assert.False(book.HasAuthor(""));
        Assert.False(book.HasAuthor("   "));
    }

    [Fact]
    public void Constructor_WithNullContributors_InitializesEmptyList()
    {
        // Arrange & Act
        var book = new Book(
            title: "The Hobbit",
            primaryAuthor: _testAuthor,
            openLibraryWorkId: TestWorkId,
            contributors: null
        );

        // Assert
        Assert.NotNull(book.Contributors);
        Assert.Empty(book.Contributors);
    }

    [Fact]
    public void GetOpenLibraryUrl_WithDifferentWorkIds_ReturnsCorrectUrls()
    {
        // Arrange & Act
        var book1 = new Book("Book 1", _testAuthor, "/works/OL12345W");
        var book2 = new Book("Book 2", _testAuthor, "/works/OL67890W");

        // Assert
        Assert.Equal("https://openlibrary.org/works/OL12345W", book1.GetOpenLibraryUrl());
        Assert.Equal("https://openlibrary.org/works/OL67890W", book2.GetOpenLibraryUrl());
    }
}