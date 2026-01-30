using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.UnitTests.Domain.ValueObjects;

public class SearchQueryTests
{
    [Fact]
    public void Constructor_WithValidQuery_CreatesSearchQuery()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien hobbit");

        // Assert
        Assert.Equal("tolkien hobbit", query.OriginalQuery);
        Assert.Equal("tolkien hobbit", query.NormalizedQuery);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("\t")]
    [InlineData("\n")]
    [InlineData("   \t  \n  ")]
    public void Constructor_WithEmptyQuery_ThrowsArgumentException(string emptyQuery)
    {
        // Arrange & Act & Assert
        var exception = Assert.Throws<ArgumentException>(() => new SearchQuery(emptyQuery));

        Assert.Equal("query", exception.ParamName);
        Assert.Contains("consulta de búsqueda no puede estar vacía", exception.Message);
    }

    [Fact]
    public void Constructor_ConvertsToLowercase()
    {
        // Arrange & Act
        var query = new SearchQuery("TOLKIEN HOBBIT");

        // Assert
        Assert.Equal("TOLKIEN HOBBIT", query.OriginalQuery);
        Assert.Equal("tolkien hobbit", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_TrimsWhitespace()
    {
        // Arrange & Act
        var query = new SearchQuery("  tolkien hobbit  ");

        // Assert
        Assert.Equal("  tolkien hobbit  ", query.OriginalQuery);
        Assert.Equal("tolkien hobbit", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_RemovesMultipleSpaces()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien    hobbit     1937");

        // Assert
        Assert.Equal("tolkien    hobbit     1937", query.OriginalQuery);
        Assert.Equal("tolkien hobbit 1937", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_HandlesTabsAndNewlines()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien\t\thobbit\n\n1937");

        // Assert
        Assert.Contains("tolkien", query.NormalizedQuery);
        Assert.Contains("hobbit", query.NormalizedQuery);
        Assert.Contains("1937", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_RemovesLeadingAndTrailingSpaces()
    {
        // Arrange & Act
        var query = new SearchQuery("   tolkien hobbit   ");

        // Assert
        Assert.Equal("tolkien hobbit", query.NormalizedQuery);
        Assert.DoesNotStartWith(" ", query.NormalizedQuery);
        Assert.DoesNotEndWith(" ", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_PreservesOriginalQuery()
    {
        // Arrange & Act
        var query = new SearchQuery("  TOLKIEN    HOBBIT  ");

        // Assert
        Assert.Equal("  TOLKIEN    HOBBIT  ", query.OriginalQuery);
    }

    [Fact]
    public void ToString_ReturnsOriginalQuery()
    {
        // Arrange
        var query = new SearchQuery("  TOLKIEN HOBBIT  ");

        // Act
        var result = query.ToString();

        // Assert
        Assert.Equal("  TOLKIEN HOBBIT  ", result);
    }

    [Fact]
    public void RecordEquality_SameValues_AreEqual()
    {
        // Arrange
        var query1 = new SearchQuery("tolkien hobbit");
        var query2 = new SearchQuery("tolkien hobbit");

        // Act & Assert
        Assert.Equal(query1, query2);
        Assert.True(query1 == query2);
        Assert.False(query1 != query2);
    }

    [Fact]
    public void RecordEquality_DifferentQueries_AreNotEqual()
    {
        // Arrange
        var query1 = new SearchQuery("tolkien hobbit");
        var query2 = new SearchQuery("martin game of thrones");

        // Act & Assert
        Assert.NotEqual(query1, query2);
        Assert.False(query1 == query2);
        Assert.True(query1 != query2);
    }

    [Fact]
    public void RecordEquality_DifferentCasing_AreNotEqual()
    {
        // Arrange
        var query1 = new SearchQuery("tolkien hobbit");
        var query2 = new SearchQuery("TOLKIEN HOBBIT");

        // Act & Assert
        // Note: While normalized queries are the same, original queries differ
        Assert.NotEqual(query1, query2);
    }

    [Fact]
    public void GetHashCode_SameValues_ReturnsSameHashCode()
    {
        // Arrange
        var query1 = new SearchQuery("tolkien hobbit");
        var query2 = new SearchQuery("tolkien hobbit");

        // Act & Assert
        Assert.Equal(query1.GetHashCode(), query2.GetHashCode());
    }

    [Fact]
    public void Constructor_WithSingleWord_CreatesSearchQuery()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien");

        // Assert
        Assert.Equal("tolkien", query.OriginalQuery);
        Assert.Equal("tolkien", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithVeryLongQuery_CreatesSearchQuery()
    {
        // Arrange
        var longQuery = new string('A', 1000) + " " + new string('B', 1000);

        // Act
        var query = new SearchQuery(longQuery);

        // Assert
        Assert.Equal(longQuery, query.OriginalQuery);
        Assert.NotEmpty(query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithSpecialCharacters_PreservesSpecialCharacters()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien's hobbit: an unexpected journey!");

        // Assert
        Assert.Equal("tolkien's hobbit: an unexpected journey!", query.OriginalQuery);
        Assert.Equal("tolkien's hobbit: an unexpected journey!", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithNumbers_PreservesNumbers()
    {
        // Arrange & Act
        var query = new SearchQuery("tolkien hobbit 1937");

        // Assert
        Assert.Equal("tolkien hobbit 1937", query.OriginalQuery);
        Assert.Equal("tolkien hobbit 1937", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithUnicodeCharacters_PreservesUnicode()
    {
        // Arrange & Act
        var query = new SearchQuery("José García Márquez");

        // Assert
        Assert.Equal("José García Márquez", query.OriginalQuery);
        Assert.Equal("josé garcía márquez", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithMixedSpacingAndCasing_NormalizesCorrectly()
    {
        // Arrange & Act
        var query = new SearchQuery("  TOLKIEN    hobbit  ILLUSTRATED   deluxe  ");

        // Assert
        Assert.Equal("tolkien hobbit illustrated deluxe", query.NormalizedQuery);
    }

    [Fact]
    public void NormalizedQuery_DoesNotContainDoubleSpaces()
    {
        // Arrange & Act
        var query = new SearchQuery("a  b   c    d     e");

        // Assert
        Assert.DoesNotContain("  ", query.NormalizedQuery);
    }

    [Fact]
    public void Constructor_WithOnlySpacesBetweenWords_CollapsesToSingleSpaces()
    {
        // Arrange & Act
        var query = new SearchQuery("word1          word2");

        // Assert
        Assert.Equal("word1 word2", query.NormalizedQuery);
    }

    [Fact]
    public void RecordEquality_WithWhitespaceVariations_HandlesCorrectly()
    {
        // Arrange
        var query1 = new SearchQuery("tolkien  hobbit");
        var query2 = new SearchQuery("tolkien   hobbit");

        // Act & Assert
        // Different original queries mean they're not equal
        Assert.NotEqual(query1, query2);
    }
}