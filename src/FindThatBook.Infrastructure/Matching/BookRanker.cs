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

    public BookRanker(ILogger<BookRanker> logger)
    {
        _logger = logger;
    }

    public List<BookMatch> RankAndLimit(List<BookMatch> matches, int topN = 5)
    {
        if (matches == null || !matches.Any())
        {
            _logger.LogInformation("No matches to rank");
            return new List<BookMatch>();
        }

        _logger.LogInformation("Ranking {Count} matches, limiting to top {TopN}", matches.Count, topN);

        // Ordenar por MatchStrength (más fuerte = 1, muy débil = 5) y, a continuación, por puntuación descendente.
        var ranked = matches
            .OrderBy(m => m.Strength)        // Primero Strongest 
            .ThenByDescending(m => m.Score)  // Puntuación más alta primero dentro de la misma strength
            .Take(topN)
            .ToList();

        _logger.LogInformation("Returning {Count} top matches", ranked.Count);

        return ranked;
    }
}