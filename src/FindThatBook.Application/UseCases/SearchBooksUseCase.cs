using FindThatBook.Application.DTOs;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace FindThatBook.Application.UseCases;

public class SearchBooksUseCase
{
    private readonly IAIFieldExtractor _aiExtractor;
    private readonly IOpenLibraryClient _openLibraryClient;
    private readonly IBookMatcher _matcher;
    private readonly IBookRanker _ranker;
    private readonly ILogger<SearchBooksUseCase> _logger;

    public SearchBooksUseCase(
        IAIFieldExtractor aiExtractor,
        IOpenLibraryClient openLibraryClient,
        IBookMatcher matcher,
        IBookRanker ranker,
        ILogger<SearchBooksUseCase> logger)
    {
        _aiExtractor = aiExtractor;
        _openLibraryClient = openLibraryClient;
        _matcher = matcher;
        _ranker = ranker;
        _logger = logger;
    }

    public async Task<SearchBooksResponse> ExecuteAsync(
        SearchBooksRequest request,
        CancellationToken cancellationToken = default)
    {
        // Validar request
        if (request is null)
            throw new ArgumentNullException(nameof(request));

        if (string.IsNullOrWhiteSpace(request.Query))
            throw new ArgumentException("Query cannot be empty.", nameof(request));

        _logger.LogInformation("Starting book search for query length: {Length}", request.Query.Length);

        // Step 1: Extraer campos con AI (con manejo de errores)
        AIExtractionResult extraction;
        try
        {
            extraction = await _aiExtractor.ExtractFieldsAsync(request.Query, cancellationToken);
        }
        catch (AIExtractionException ex)
        {
            _logger.LogWarning(ex, "AI extraction failed, returning empty results");
            return CreateEmptyResponse(request.Query, "AI service temporarily unavailable. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "AI service unavailable");
            return CreateEmptyResponse(request.Query, "AI service temporarily unavailable. Please try again.");
        }

        // Validar que AI extrajo algo útil
        if (!extraction.HasAnyField)
        {
            _logger.LogInformation("No useful fields extracted from query");
            return new SearchBooksResponse
            {
                Query = request.Query,
                Extraction = extraction,
                Results = new List<BookResultDto>(),
                Message = "Could not extract book information from your query. Try including a title or author name."
            };
        }

        var keywords = extraction.Keywords?.Any() == true
            ? string.Join(", ", extraction.Keywords)
            : "none";

        _logger.LogInformation(
            "AI extracted - Title: {Title}, Author: {Author}, Keywords: {Keywords}",
            extraction.Title ?? "none",
            extraction.Author ?? "none",
            keywords);

        // Step 2: Buscar en Open Library (con manejo de errores)
        List<Book> candidates;
        try
        {
            candidates = await SearchCandidatesAsync(extraction, cancellationToken);
        }
        catch (OpenLibraryApiException ex)
        {
            _logger.LogWarning(ex, "Open Library API error");
            return CreateResponseWithExtraction(request.Query, extraction,
                "Book search service temporarily unavailable. Please try again.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogWarning(ex, "Open Library service unavailable");
            return CreateResponseWithExtraction(request.Query, extraction,
                "Book search service temporarily unavailable. Please try again.");
        }

        _logger.LogInformation("Found {Count} candidates from Open Library", candidates.Count);

        // Si no hay candidatos, devolver respuesta vacía con mensaje útil
        if (!candidates.Any())
        {
            var searchTerms = BuildSearchTermsMessage(extraction);
            return new SearchBooksResponse
            {
                Query = request.Query,
                Extraction = extraction,
                Results = new List<BookResultDto>(),
                Message = $"No books found matching {searchTerms}. Try different search terms."
            };
        }

        // Step 3: Match and rank
        var matches = _matcher.Match(extraction, candidates);

        // Si no hay matches, devolver candidatos sin ranking
        if (!matches.Any())
        {
            _logger.LogInformation("No matches found, returning top candidates as suggestions");
            return new SearchBooksResponse
            {
                Query = request.Query,
                Extraction = extraction,
                Results = candidates.Take(5).Select(c => new BookResultDto
                {
                    Title = c.Title,
                    Author = c.PrimaryAuthor.Name,
                    FirstPublishYear = c.FirstPublishYear,
                    OpenLibraryId = c.OpenLibraryWorkId,
                    OpenLibraryUrl = c.GetOpenLibraryUrl(),
                    CoverUrl = c.CoverUrl,
                    Explanation = "Potential match based on search terms."
                }).ToList(),
                Message = "No exact matches found. Showing potential matches."
            };
        }

        var topMatches = _ranker.RankAndLimit(matches, topN: 5);

        _logger.LogInformation("Returning {Count} top matches", topMatches.Count);

        // Step 4: Mapa a la respuesta DTO
        return new SearchBooksResponse
        {
            Query = request.Query,
            Extraction = extraction,
            Results = topMatches.Select(m => new BookResultDto
            {
                Title = m.Book.Title,
                Author = m.Book.PrimaryAuthor.Name,
                FirstPublishYear = m.Book.FirstPublishYear,
                OpenLibraryId = m.Book.OpenLibraryWorkId,
                OpenLibraryUrl = m.Book.GetOpenLibraryUrl(),
                CoverUrl = m.Book.CoverUrl,
                Explanation = m.Explanation
            }).ToList()
        };
    }

    private async Task<List<Book>> SearchCandidatesAsync(
        AIExtractionResult extraction,
        CancellationToken cancellationToken)
    {
        if (extraction.HasTitle || extraction.HasAuthor)
        {
            return await _openLibraryClient.SearchBooksAsync(
                extraction.Title,
                extraction.Author,
                limit: 20,
                cancellationToken);
        }

        if (extraction.Keywords?.Any() == true)
        {
            var keywordQuery = string.Join(" ", extraction.Keywords);
            return await _openLibraryClient.SearchBooksAsync(
                title: keywordQuery,
                author: null,
                limit: 20,
                cancellationToken);
        }

        _logger.LogWarning("No fields extracted from query, returning empty results");
        return new List<Book>();
    }

    private static SearchBooksResponse CreateEmptyResponse(string query, string message)
    {
        return new SearchBooksResponse
        {
            Query = query,
            Extraction = new AIExtractionResult(),
            Results = new List<BookResultDto>(),
            Message = message
        };
    }

    private static SearchBooksResponse CreateResponseWithExtraction(
        string query,
        AIExtractionResult extraction,
        string message)
    {
        return new SearchBooksResponse
        {
            Query = query,
            Extraction = extraction,
            Results = new List<BookResultDto>(),
            Message = message
        };
    }

    private static string BuildSearchTermsMessage(AIExtractionResult extraction)
    {
        var parts = new List<string>();

        if (extraction.HasTitle)
            parts.Add($"title '{extraction.Title}'");

        if (extraction.HasAuthor)
            parts.Add($"author '{extraction.Author}'");

        if (extraction.Keywords?.Any() == true)
            parts.Add($"keywords '{string.Join(", ", extraction.Keywords)}'");

        return parts.Any() ? string.Join(" and ", parts) : "your search terms";
    }
}