using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using FindThatBook.Application.DTOs;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Records;
using Microsoft.Extensions.Logging;

namespace FindThatBook.Infrastructure.Matching;

/// <summary>
/// Aplica estrategias de matching (título+autor, solo título, solo autor, keywords) con normalización.
/// </summary>
public class BookMatcher : IBookMatcher
{
    private readonly ILogger<BookMatcher> _logger;

    // Artículos comunes en inglés y español para remover
    private static readonly HashSet<string> Articles = new(StringComparer.OrdinalIgnoreCase)
    {
        "the", "a", "an", "el", "la", "los", "las", "un", "una", "unos", "unas"
    };

    public BookMatcher(ILogger<BookMatcher> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Valida entrada, evalúa cada candidato y acumula resultados con logging.
    /// </summary>
    public List<BookMatch> Match(AIExtractionResult extraction, List<Book> candidates)
    {
        if (extraction == null)
        {
            _logger.LogWarning("Null extraction result provided");
            return new List<BookMatch>();
        }

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

    /// <summary>
    /// Evalúa un candidato según la jerarquía: Strongest/Strong/Medium/Weak/VeryWeak.
    /// </summary>
    private BookMatch? TryMatchBook(AIExtractionResult extraction, Book candidate)
    {
        // Strategy 1: Titulo + Autor (Más fuerte/Fuerte)
        if (extraction.HasTitle && extraction.HasAuthor)
        {
            var titleMatch = IsTitleMatch(extraction.Title!, candidate.Title);
            var authorMatch = candidate.HasAuthor(extraction.Author!);

            if (titleMatch.IsMatch && authorMatch)
            {
                var normalizedSearchAuthor = extraction.Author!.ToLowerInvariant();
                var isPrimaryAuthor = RemoveDiacritics(candidate.PrimaryAuthor.GetNormalizedName())
                    .Contains(RemoveDiacritics(normalizedSearchAuthor));

                if (isPrimaryAuthor)
                {
                    return BookMatch.CreateStrongest(candidate, candidate.PrimaryAuthor.Name);
                }
                else
                {
                    var matchedContributor = candidate.Contributors
                        .FirstOrDefault(c => RemoveDiacritics(c.GetNormalizedName())
                            .Contains(RemoveDiacritics(normalizedSearchAuthor)));

                    var contributorName = matchedContributor?.Name ?? extraction.Author!;
                    return BookMatch.CreateStrong(candidate, contributorName, "contributor");
                }
            }

            // Coincidencia parcial: título coincide pero autor no
            if (titleMatch.IsMatch)
            {
                return BookMatch.CreateMedium(
                    candidate,
                    candidate.Title,
                    similarity: titleMatch.Similarity);
            }
        }

        // Strategy 2: Solo titulo
        if (extraction.HasTitle && !extraction.HasAuthor)
        {
            var titleMatch = IsTitleMatch(extraction.Title!, candidate.Title);

            if (titleMatch.IsMatch)
            {
                return BookMatch.CreateMedium(
                    candidate,
                    candidate.Title,
                    similarity: titleMatch.Similarity);
            }
        }

        // Strategy 3: Solo autor (fallback)
        if (extraction.HasAuthor && !extraction.HasTitle)
        {
            var authorMatch = candidate.HasAuthor(extraction.Author!);

            if (authorMatch)
            {
                return BookMatch.CreateWeak(candidate, candidate.PrimaryAuthor.Name);
            }
        }

        // Strategy 4: Keywords match
        if (extraction.Keywords?.Any() == true)
        {
            var matchedKeywords = extraction.Keywords
                .Where(keyword =>
                    NormalizeForComparison(candidate.Title).Contains(NormalizeForComparison(keyword)))
                .ToList();

            if (matchedKeywords.Any())
            {
                return BookMatch.CreateVeryWeak(candidate, matchedKeywords);
            }
        }

        // Sin match
        return null;
    }

    /// <summary>
    /// Resultado del matching de títulos con información de similitud
    /// </summary>
    private record TitleMatchResult(bool IsMatch, double Similarity);

    /// <summary>
    /// Compara títulos manejando variantes, subtítulos, artículos y diacríticos
    /// </summary>
    private TitleMatchResult IsTitleMatch(string searchTitle, string candidateTitle)
    {
        var normalizedSearch = NormalizeForComparison(searchTitle);
        var normalizedCandidate = NormalizeForComparison(candidateTitle);

        // 1. Match exacto después de normalización
        if (normalizedSearch == normalizedCandidate)
            return new TitleMatchResult(true, 1.0);

        // 2. Uno contiene al otro completamente
        if (normalizedCandidate.Contains(normalizedSearch))
            return new TitleMatchResult(true, 0.95);

        if (normalizedSearch.Contains(normalizedCandidate))
            return new TitleMatchResult(true, 0.90);

        // 3. Match sin artículos (The Hobbit vs Hobbit)
        var searchWithoutArticles = RemoveArticles(normalizedSearch);
        var candidateWithoutArticles = RemoveArticles(normalizedCandidate);

        if (searchWithoutArticles == candidateWithoutArticles)
            return new TitleMatchResult(true, 0.95);

        if (candidateWithoutArticles.Contains(searchWithoutArticles) ||
            searchWithoutArticles.Contains(candidateWithoutArticles))
            return new TitleMatchResult(true, 0.85);

        // 4. Match de subtítulos (Book: Subtitle o Book - Subtitle)
        var candidateMainTitle = ExtractMainTitle(normalizedCandidate);
        var searchMainTitle = ExtractMainTitle(normalizedSearch);

        if (candidateMainTitle == searchMainTitle ||
            candidateMainTitle.Contains(searchMainTitle) ||
            searchMainTitle.Contains(candidateMainTitle))
            return new TitleMatchResult(true, 0.80);

        // 5. Similitud basada en palabras (Jaccard similarity mejorado)
        var similarity = CalculateWordSimilarity(normalizedSearch, normalizedCandidate);

        if (similarity >= 0.6)
            return new TitleMatchResult(true, similarity);

        // 6. Al menos la mitad de las palabras de búsqueda están en el candidato
        var searchWords = GetSignificantWords(normalizedSearch);
        var candidateWords = GetSignificantWords(normalizedCandidate);

        if (searchWords.Any())
        {
            var matchedCount = searchWords.Count(sw =>
                candidateWords.Any(cw => cw.Contains(sw) || sw.Contains(cw)));

            var matchRatio = (double)matchedCount / searchWords.Count;

            if (matchRatio >= 0.5)
                return new TitleMatchResult(true, 0.5 + (matchRatio * 0.3));
        }

        return new TitleMatchResult(false, 0);
    }

    /// <summary>
    /// Normaliza texto para comparación: lowercase, sin acentos, sin puntuación
    /// </summary>
    private static string NormalizeForComparison(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Lowercase
        var normalized = text.ToLowerInvariant();

        // Remover diacríticos (acentos)
        normalized = RemoveDiacritics(normalized);

        // Remover puntuación excepto espacios
        normalized = Regex.Replace(normalized, @"[^\w\s]", " ");

        // Normalizar espacios múltiples
        normalized = Regex.Replace(normalized, @"\s+", " ");

        return normalized.Trim();
    }

    /// <summary>
    /// Remueve artículos comunes del texto
    /// </summary>
    private static string RemoveArticles(string text)
    {
        var words = text.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        var filteredWords = words.Where(w => !Articles.Contains(w));
        return string.Join(" ", filteredWords);
    }

    /// <summary>
    /// Extrae el título principal antes de separadores comunes (: - –)
    /// "The Hobbit: There and Back Again" -> "the hobbit"
    /// </summary>
    private static string ExtractMainTitle(string title)
    {
        // Separadores comunes de subtítulos
        var separators = new[] { ":", " - ", " – ", " — ", " | " };

        foreach (var separator in separators)
        {
            var index = title.IndexOf(separator, StringComparison.Ordinal);
            if (index > 0)
            {
                return title.Substring(0, index).Trim();
            }
        }

        return title;
    }

    /// <summary>
    /// Obtiene palabras significativas (sin artículos, más de 2 caracteres)
    /// </summary>
    private static List<string> GetSignificantWords(string text)
    {
        return text.Split(' ', StringSplitOptions.RemoveEmptyEntries)
            .Where(w => w.Length > 2 && !Articles.Contains(w))
            .ToList();
    }

    /// <summary>
    /// Calcula similitud entre dos strings basada en palabras (Jaccard)
    /// </summary>
    private static double CalculateWordSimilarity(string s1, string s2)
    {
        var words1 = GetSignificantWords(s1).ToHashSet();
        var words2 = GetSignificantWords(s2).ToHashSet();

        if (!words1.Any() || !words2.Any())
            return 0;

        var intersection = words1.Intersect(words2).Count();
        var union = words1.Union(words2).Count();

        return (double)intersection / union;
    }

    /// <summary>
    /// Remueve acentos y diacríticos
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        if (string.IsNullOrEmpty(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder(normalizedString.Length);

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }
}