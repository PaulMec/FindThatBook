using FindThatBook.Domain.Exceptions;

namespace FindThatBook.UnitTests.Domain.Exceptions;

public class BookSearchExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_CreatesException()
    {
        // Arrange
        var message = "Search failed";

        // Act
        var exception = new BookSearchException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesException()
    {
        // Arrange
        var message = "Search failed";
        var innerException = new InvalidOperationException("Inner error");

        // Act
        var exception = new BookSearchException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void BookSearchException_IsException()
    {
        // Arrange & Act
        var exception = new BookSearchException("Test");

        // Assert
        Assert.IsAssignableFrom<Exception>(exception);
    }

    [Fact]
    public void Constructor_WithEmptyMessage_CreatesException()
    {
        // Arrange & Act
        var exception = new BookSearchException(string.Empty);

        // Assert
        Assert.Equal(string.Empty, exception.Message);
    }
}

public class AIExtractionExceptionTests
{
    [Fact]
    public void Constructor_WithQueryAndMessage_CreatesException()
    {
        // Arrange
        var query = "tolkien hobbit";
        var message = "Failed to extract fields";

        // Act
        var exception = new AIExtractionException(query, message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(query, exception.Query);
        Assert.Null(exception.AIResponse);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithQueryMessageAndAIResponse_CreatesException()
    {
        // Arrange
        var query = "tolkien hobbit";
        var message = "Failed to parse AI response";
        var aiResponse = "{invalid json}";

        // Act
        var exception = new AIExtractionException(query, message, aiResponse);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(query, exception.Query);
        Assert.Equal(aiResponse, exception.AIResponse);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithQueryMessageAndInnerException_CreatesException()
    {
        // Arrange
        var query = "tolkien hobbit";
        var message = "AI service error";
        var innerException = new HttpRequestException("Network error");

        // Act
        var exception = new AIExtractionException(query, message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(query, exception.Query);
        Assert.Null(exception.AIResponse);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void AIExtractionException_IsBookSearchException()
    {
        // Arrange & Act
        var exception = new AIExtractionException("query", "message");

        // Assert
        Assert.IsAssignableFrom<BookSearchException>(exception);
    }

    [Fact]
    public void Constructor_WithEmptyQuery_CreatesException()
    {
        // Arrange & Act
        var exception = new AIExtractionException(string.Empty, "message");

        // Assert
        Assert.Equal(string.Empty, exception.Query);
    }

    [Fact]
    public void Constructor_PreservesComplexQuery()
    {
        // Arrange
        var complexQuery = "tolkien hobbit illustrated deluxe edition 1937 with maps";

        // Act
        var exception = new AIExtractionException(complexQuery, "message");

        // Assert
        Assert.Equal(complexQuery, exception.Query);
    }

    [Fact]
    public void Constructor_PreservesComplexAIResponse()
    {
        // Arrange
        var complexResponse = @"{""title"": ""The Hobbit"", ""author"": ""Tolkien"", ""year"": 1937}";

        // Act
        var exception = new AIExtractionException("query", "message", complexResponse);

        // Assert
        Assert.Equal(complexResponse, exception.AIResponse);
    }
}

public class OpenLibraryApiExceptionTests
{
    [Fact]
    public void Constructor_WithMessage_CreatesException()
    {
        // Arrange
        var message = "API call failed";

        // Act
        var exception = new OpenLibraryApiException(message);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.Endpoint);
        Assert.Null(exception.StatusCode);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageEndpointAndStatusCode_CreatesException()
    {
        // Arrange
        var message = "API returned error";
        var endpoint = "/search.json";
        var statusCode = 500;

        // Act
        var exception = new OpenLibraryApiException(message, endpoint, statusCode);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Equal(endpoint, exception.Endpoint);
        Assert.Equal(statusCode, exception.StatusCode);
        Assert.Null(exception.InnerException);
    }

    [Fact]
    public void Constructor_WithMessageAndInnerException_CreatesException()
    {
        // Arrange
        var message = "Network error";
        var innerException = new HttpRequestException("Connection timeout");

        // Act
        var exception = new OpenLibraryApiException(message, innerException);

        // Assert
        Assert.Equal(message, exception.Message);
        Assert.Null(exception.Endpoint);
        Assert.Null(exception.StatusCode);
        Assert.Equal(innerException, exception.InnerException);
    }

    [Fact]
    public void OpenLibraryApiException_IsBookSearchException()
    {
        // Arrange & Act
        var exception = new OpenLibraryApiException("message");

        // Assert
        Assert.IsAssignableFrom<BookSearchException>(exception);
    }

    [Fact]
    public void Constructor_WithDifferentStatusCodes_PreservesStatusCode()
    {
        // Arrange & Act
        var exception404 = new OpenLibraryApiException("Not found", "/search.json", 404);
        var exception500 = new OpenLibraryApiException("Server error", "/search.json", 500);
        var exception429 = new OpenLibraryApiException("Rate limit", "/search.json", 429);

        // Assert
        Assert.Equal(404, exception404.StatusCode);
        Assert.Equal(500, exception500.StatusCode);
        Assert.Equal(429, exception429.StatusCode);
    }

    [Fact]
    public void Constructor_WithFullEndpointUrl_PreservesEndpoint()
    {
        // Arrange
        var endpoint = "https://openlibrary.org/search.json?q=tolkien";

        // Act
        var exception = new OpenLibraryApiException("Error", endpoint, 500);

        // Assert
        Assert.Equal(endpoint, exception.Endpoint);
    }

    [Fact]
    public void Constructor_WithEmptyEndpoint_PreservesEmptyString()
    {
        // Arrange & Act
        var exception = new OpenLibraryApiException("Error", string.Empty, 500);

        // Assert
        Assert.Equal(string.Empty, exception.Endpoint);
    }

    [Fact]
    public void Constructor_WithZeroStatusCode_PreservesZero()
    {
        // Arrange & Act
        var exception = new OpenLibraryApiException("Error", "/search.json", 0);

        // Assert
        Assert.Equal(0, exception.StatusCode);
    }

    [Fact]
    public void Constructor_WithNegativeStatusCode_PreservesValue()
    {
        // Arrange & Act
        var exception = new OpenLibraryApiException("Error", "/search.json", -1);

        // Assert
        Assert.Equal(-1, exception.StatusCode);
    }
}

public class ExceptionHierarchyTests
{
    [Fact]
    public void AIExtractionException_CanBeCaughtAsBookSearchException()
    {
        // Arrange
        Exception caughtException = null!;

        // Act
        try
        {
            throw new AIExtractionException("query", "message");
        }
        catch (BookSearchException ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<AIExtractionException>(caughtException);
    }

    [Fact]
    public void OpenLibraryApiException_CanBeCaughtAsBookSearchException()
    {
        // Arrange
        Exception caughtException = null!;

        // Act
        try
        {
            throw new OpenLibraryApiException("message");
        }
        catch (BookSearchException ex)
        {
            caughtException = ex;
        }

        // Assert
        Assert.NotNull(caughtException);
        Assert.IsType<OpenLibraryApiException>(caughtException);
    }

    [Fact]
    public void AllExceptions_CanBeCaughtAsException()
    {
        // Arrange & Act & Assert
        Assert.Throws<Exception>(() => throw new BookSearchException("message"));
        Assert.Throws<Exception>(() => throw new AIExtractionException("query", "message"));
        Assert.Throws<Exception>(() => throw new OpenLibraryApiException("message"));
    }

    [Fact]
    public void ExceptionTypes_AreSealed()
    {
        // Assert
        Assert.True(typeof(AIExtractionException).IsSealed);
        Assert.True(typeof(OpenLibraryApiException).IsSealed);
    }
}