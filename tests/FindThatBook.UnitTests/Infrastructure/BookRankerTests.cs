using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Records;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Infrastructure.Matching;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace FindThatBook.UnitTests.Infrastructure;

public class BookRankerTests
{
    private readonly BookRanker _ranker;

    public BookRankerTests()
    {
        var loggerMock = new Mock<ILogger<BookRanker>>();
        var deduplicatorLoggerMock = new Mock<ILogger<BookDeduplicator>>();
        var deduplicator = new BookDeduplicator(deduplicatorLoggerMock.Object);
        _ranker = new BookRanker(loggerMock.Object, deduplicator);
    }

    private Book CreateTestBook(string title)
    {
        return new Book(
            title: title,
            primaryAuthor: new Author("Test Author"),
            openLibraryWorkId: "OL123W"
        );
    }

    [Fact]
    public void RankAndLimit_ShouldOrderByStrengthFirst()
    {
        // Arrange
        var matches = new List<BookMatch>
        {
            BookMatch.CreateWeak(CreateTestBook("Weak Match"), "Author"),
            BookMatch.CreateStrongest(CreateTestBook("Strongest Match"), "Author"),
            BookMatch.CreateMedium(CreateTestBook("Medium Match"), "Title", 0.7)
        };

        // Act
        var ranked = _ranker.RankAndLimit(matches);

        // Assert
        ranked.First().Book.Title.Should().Be("Strongest Match");
        ranked.Last().Book.Title.Should().Be("Weak Match");
    }

    [Fact]
    public void RankAndLimit_WithLimit_ShouldReturnRequestedCount()
    {
        // Arrange
        var matches = new List<BookMatch>();
        for (int i = 0; i < 10; i++)
        {
            matches.Add(BookMatch.CreateMedium(CreateTestBook($"Book {i}"), "Title", 0.5));
        }

        // Act
        var ranked = _ranker.RankAndLimit(matches, topN: 3);

        // Assert
        ranked.Should().HaveCount(3);
    }

    [Fact]
    public void RankAndLimit_WithEmptyList_ShouldReturnEmpty()
    {
        // Arrange
        var matches = new List<BookMatch>();

        // Act
        var ranked = _ranker.RankAndLimit(matches);

        // Assert
        ranked.Should().BeEmpty();
    }

    [Fact]
    public void RankAndLimit_WithNullList_ShouldReturnEmpty()
    {
        // Act
        var ranked = _ranker.RankAndLimit(null!);

        // Assert
        ranked.Should().BeEmpty();
    }
}