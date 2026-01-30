
using FindThatBook.Application.DTOs;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Entities;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
        _logger.LogInformation("Starting book search for query: {Query}", request.Query);

        // Step 1: Extraer campos utilizando IA
        var extraction = await _aiExtractor.ExtractFieldsAsync(request.Query, cancellationToken);

        _logger.LogInformation(
            "AI extracted - Title: {Title}, Author: {Author}, Keywords: {Keywords}",
            extraction.Title ?? "none",
            extraction.Author ?? "none",
            string.Join(", ", extraction.Keywords));

        // Step 2: Buscar en Open Library
        var candidates = await SearchCandidatesAsync(extraction, cancellationToken);

        _logger.LogInformation("Found {Count} candidates from Open Library", candidates.Count);

        // Step 3: Match and rank
        var matches = _matcher.Match(extraction, candidates);
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
        // Si tenemos el título y/o el autor, busque normalmente.
        if (extraction.HasTitle || extraction.HasAuthor)
        {
            return await _openLibraryClient.SearchBooksAsync(
                extraction.Title,
                extraction.Author,
                limit: 20,
                cancellationToken);
        }

        // Si solo hay palabras clave, busca con palabras clave como consulta.
        if (extraction.Keywords.Any())
        {
            var keywordQuery = string.Join(" ", extraction.Keywords);
            return await _openLibraryClient.SearchBooksAsync(
                title: keywordQuery,
                author: null,
                limit: 20,
                cancellationToken);
        }

        // No se han extraído campos: devuelve vacío.
        _logger.LogWarning("No fields extracted from query, returning empty results");
        return new List<Book>();
    }
}