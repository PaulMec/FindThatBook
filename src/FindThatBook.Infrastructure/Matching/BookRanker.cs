using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Records;
using Microsoft.Extensions.Logging;

namespace FindThatBook.Infrastructure.Matching;

public class BookRanker : IBookRanker
{
    private readonly ILogger<BookRanker> _logger;
    private readonly BookDeduplicator _deduplicator;

    public BookRanker(ILogger<BookRanker> logger, BookDeduplicator deduplicator)
    {
        _logger = logger;
        _deduplicator = deduplicator;
    }

    public List<BookMatch> RankAndLimit(List<BookMatch> matches, int topN = 5)
    {
        if (matches == null || !matches.Any())
        {
            _logger.LogInformation("No matches to rank");
            return new List<BookMatch>();
        }

        _logger.LogInformation("Ranking {Count} matches, limiting to top {TopN}", matches.Count, topN);

        // Paso 1: Deduplicar
        var deduplicated = _deduplicator.Deduplicate(matches);

        // Paso 2: Ordenar por Score (descendente) y luego por Strength
        var ranked = deduplicated
            .OrderByDescending(m => m.Score)
            .ThenByDescending(m => (int)m.Strength)
            .ThenBy(m => m.Book.FirstPublishYear ?? int.MaxValue) // Más antiguo primero
            .Take(topN)
            .ToList();

        _logger.LogInformation("Returning {Count} top matches after deduplication", ranked.Count);

        return ranked;
    }
}