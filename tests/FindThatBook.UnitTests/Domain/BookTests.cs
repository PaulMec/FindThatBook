using FindThatBook.Domain.Entities;
using FindThatBook.Domain.ValueObjects;
using FluentAssertions;

namespace FindThatBook.UnitTests.Domain;

public class BookTests
{
    [Fact]
    public void Book_ShouldCreateWithValidProperties()
    {
        // Arrange
        var author = new Author("Gabriel García Márquez");

        // Act
        var book = new Book(
            title: "Cien años de soledad",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W",
            firstPublishYear: 1967
        );

        // Assert
        book.Title.Should().Be("Cien años de soledad");
        book.PrimaryAuthor.Name.Should().Be("Gabriel García Márquez");
        book.FirstPublishYear.Should().Be(1967);
    }

    [Fact]
    public void Book_GetOpenLibraryUrl_ShouldReturnCorrectUrl()
    {
        // Arrange
        var author = new Author("Test Author");
        var book = new Book(
            title: "Test Book",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Act
        var url = book.GetOpenLibraryUrl();

        // Assert
        url.Should().Be("https://openlibrary.org/works/OL123W");
    }

    [Fact]
    public void Book_GetOpenLibraryUrl_WithSlashPrefix_ShouldReturnCorrectUrl()
    {
        // Arrange
        var author = new Author("Test Author");
        var book = new Book(
            title: "Test Book",
            primaryAuthor: author,
            openLibraryWorkId: "/works/OL456W"
        );

        // Act
        var url = book.GetOpenLibraryUrl();

        // Assert
        url.Should().Be("https://openlibrary.org/works/OL456W");
    }

    [Fact]
    public void Book_HasAuthor_WithPrimaryAuthor_ShouldReturnTrue()
    {
        // Arrange
        var author = new Author("Gabriel García Márquez");
        var book = new Book(
            title: "Cien años de soledad",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Act
        var result = book.HasAuthor("garcia marquez");

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void Book_HasAuthor_WithNonMatchingAuthor_ShouldReturnFalse()
    {
        // Arrange
        var author = new Author("Gabriel García Márquez");
        var book = new Book(
            title: "Cien años de soledad",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Act
        var result = book.HasAuthor("Stephen King");

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void Book_GetNormalizedTitle_ShouldReturnLowerCase()
    {
        // Arrange
        var author = new Author("Test Author");
        var book = new Book(
            title: "The HOBBIT",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Act
        var normalized = book.GetNormalizedTitle();

        // Assert
        normalized.Should().Be("the hobbit");
    }

    [Fact]
    public void Book_GetNormalizedTitle_ShouldRemoveDiacritics()
    {
        // Arrange
        var author = new Author("Test Author");
        var book = new Book(
            title: "Cien años de soledad",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Act
        var normalized = book.GetNormalizedTitle();

        // Assert
        normalized.Should().Be("cien anos de soledad");
    }

    [Fact]
    public void Book_WithEmptyTitle_ShouldThrowArgumentException()
    {
        // Arrange
        var author = new Author("Test Author");

        // Act
        var act = () => new Book(
            title: "",
            primaryAuthor: author,
            openLibraryWorkId: "OL123W"
        );

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}