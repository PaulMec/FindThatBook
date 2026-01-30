using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FindThatBook.Application.DTOs;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;
using FindThatBook.Domain.Records;
using Microsoft.Extensions.Logging;

namespace FindThatBook.Infrastructure.Matching;

public class BookMatcher : IBookMatcher
{
    private readonly ILogger<BookMatcher> _logger;

    public BookMatcher(ILogger<BookMatcher> logger)
    {
        _logger = logger;
    }

    public List<BookMatch> Match(AIExtractionResult extraction, List<Book> candidates)
    {
        if (candidates == null || !candidates.Any())
        {
            _logger.LogInformation("No candidates to match");
            return new List<BookMatch>();
        }

        _logger.LogInformation("Matching {Count} candidates against extraction", candidates.Count);

        var matches = new List<BookMatch>();

        foreach (var candidate in candidates)
        {
            var match = TryMatchBook(extraction, candidate);
            if (match != null)
            {
                matches.Add(match);
            }
        }

        _logger.LogInformation("Found {Count} matches", matches.Count);

        return matches;
    }

    private BookMatch? TryMatchBook(AIExtractionResult extraction, Book candidate)
    {
        // Strategy 1: Titulo + Autor (Más fuerte/Fuerte)
        if (extraction.HasTitle && extraction.HasAuthor)
        {
            var titleMatch = IsTitleMatch(extraction.Title!, candidate.GetNormalizedTitle());
            var authorMatch = candidate.HasAuthor(extraction.Author!);

            if (titleMatch && authorMatch)
            {
                // Comprueba si es el autor principal o colaborador.
                var isPrimaryAuthor = candidate.PrimaryAuthor.GetNormalizedName()
                    .Contains(extraction.Author!.ToLowerInvariant());

                if (isPrimaryAuthor)
                {
                    return BookMatch.CreateStrongest(candidate, extraction.Author!);
                }
                else
                {
                    return BookMatch.CreateStrong(candidate, extraction.Author!, "contributor");
                }
            }

            // Coincidencia parcial: coinciden los titulos, pero no los autores (o viceversa).
            if (titleMatch)
            {
                return BookMatch.CreateMedium(
                    candidate,
                    candidate.Title,
                    similarity: 0.7);
            }
        }

        // Strategy 2: Solo titulo
        if (extraction.HasTitle && !extraction.HasAuthor)
        {
            var titleMatch = IsTitleMatch(extraction.Title!, candidate.GetNormalizedTitle());

            if (titleMatch)
            {
                return BookMatch.CreateMedium(
                    candidate,
                    candidate.Title,
                    similarity: 0.6);
            }
        }

        // Strategy 3: Solo autor (fallback)
        if (extraction.HasAuthor && !extraction.HasTitle)
        {
            var authorMatch = candidate.HasAuthor(extraction.Author!);

            if (authorMatch)
            {
                return BookMatch.CreateWeak(candidate, extraction.Author!);
            }
        }

        // Strategy 4: Keywords match
        if (extraction.Keywords?.Any() == true)
        {
            var matchedKeywords = extraction.Keywords
                .Where(keyword => candidate.GetNormalizedTitle().Contains(keyword.ToLowerInvariant()))
                .ToList();

            if (matchedKeywords.Any())
            {
                return BookMatch.CreateVeryWeak(candidate, matchedKeywords);
            }
        }

        // Sin match
        return null;
    }

    private bool IsTitleMatch(string extractedTitle, string candidateTitle)
    {
        var normalizedExtracted = extractedTitle.ToLowerInvariant().Trim();
        var normalizedCandidate = candidateTitle.ToLowerInvariant().Trim();

        // Match exacto
        if (normalizedExtracted == normalizedCandidate)
            return true;

        // Contiene Match (e.g., "Hobbit" matches "The Hobbit")
        if (normalizedCandidate.Contains(normalizedExtracted) ||
            normalizedExtracted.Contains(normalizedCandidate))
            return true;

        // Coincidencia aproximada con similitud simple MVP
        var similarity = CalculateSimpleSimilarity(normalizedExtracted, normalizedCandidate);
        return similarity > 0.7;
    }

    private double CalculateSimpleSimilarity(string s1, string s2)
    {
        // Similitud simple basada en palabras para MVP
        var words1 = s1.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();
        var words2 = s2.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToHashSet();

        if (!words1.Any() || !words2.Any())
            return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }
}