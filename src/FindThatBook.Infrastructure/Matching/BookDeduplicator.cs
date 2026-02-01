using FindThatBook.Domain.Records;
using Microsoft.Extensions.Logging;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace FindThatBook.Infrastructure.Matching;

/// <summary>
/// Servicio para eliminar duplicados de resultados de libros.
/// Agrupa por título + autor y mantiene el mejor candidato.
/// </summary>
public class BookDeduplicator
{
    private readonly ILogger<BookDeduplicator> _logger;

    public BookDeduplicator(ILogger<BookDeduplicator> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Elimina duplicados de una lista de BookMatch, manteniendo el mejor de cada grupo.
    /// </summary>
    public List<BookMatch> Deduplicate(List<BookMatch> matches)
    {
        if (matches == null || matches.Count <= 1)
            return matches ?? new List<BookMatch>();

        _logger.LogInformation("Deduplicating {Count} matches", matches.Count);

        // Agrupar por clave normalizada (título + autor)
        var groups = matches
            .GroupBy(m => GetDeduplicationKey(m))
            .ToList();

        var deduplicated = new List<BookMatch>();

        foreach (var group in groups)
        {
            // Seleccionar el mejor candidato del grupo
            var best = SelectBestCandidate(group.ToList());
            deduplicated.Add(best);
        }

        _logger.LogInformation("Deduplicated from {Original} to {Final} matches",
            matches.Count, deduplicated.Count);

        return deduplicated;
    }

    /// <summary>
    /// Genera una clave única para agrupar libros similares.
    /// Combina título normalizado + autor principal normalizado.
    /// </summary>
    private string GetDeduplicationKey(BookMatch match)
    {
        var title = NormalizeForDedup(match.Book.Title);
        var author = NormalizeForDedup(match.Book.PrimaryAuthor.Name);

        // Remover subtítulos para agrupar variantes
        title = RemoveSubtitle(title);

        return $"{title}|{author}";
    }

    /// <summary>
    /// Selecciona el mejor candidato de un grupo de duplicados.
    /// Criterios (en orden de prioridad):
    /// 1. Mayor MatchStrength
    /// 2. Tiene cover image
    /// 3. Año de publicación más antiguo (primera edición)
    /// 4. Tiene descripción
    /// </summary>
    private BookMatch SelectBestCandidate(List<BookMatch> candidates)
    {
        if (candidates.Count == 1)
            return candidates[0];

        return candidates
            .OrderByDescending(c => (int)c.Strength)  // Mayor strength primero
            .ThenByDescending(c => c.Book.CoverUrl != null ? 1 : 0)  // Con cover primero
            .ThenBy(c => c.Book.FirstPublishYear ?? int.MaxValue)  // Año más antiguo primero
            .ThenByDescending(c => c.Book.Description != null ? 1 : 0)  // Con descripción primero
            .First();
    }

    /// <summary>
    /// Normaliza texto para comparación de duplicados.
    /// Lowercase, sin acentos, sin puntuación, sin artículos.
    /// </summary>
    private static string NormalizeForDedup(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return string.Empty;

        // Lowercase
        var normalized = text.ToLowerInvariant();

        // Remover diacríticos
        normalized = RemoveDiacritics(normalized);

        // Remover puntuación
        normalized = Regex.Replace(normalized, @"[^\w\s]", "");

        // Remover artículos comunes
        var articles = new[] { "the ", "a ", "an ", "el ", "la ", "los ", "las ", "un ", "una " };
        foreach (var article in articles)
        {
            if (normalized.StartsWith(article))
            {
                normalized = normalized.Substring(article.Length);
                break;
            }
        }

        // Normalizar espacios
        normalized = Regex.Replace(normalized, @"\s+", " ").Trim();

        return normalized;
    }

    /// <summary>
    /// Remueve subtítulos (después de : o -)
    /// "The Hobbit: There and Back Again" -> "the hobbit"
    /// </summary>
    private static string RemoveSubtitle(string title)
    {
        var separators = new[] { ":", " - ", " – " };

        foreach (var sep in separators)
        {
            var index = title.IndexOf(sep, StringComparison.Ordinal);
            if (index > 0)
            {
                return title.Substring(0, index).Trim();
            }
        }

        return title;
    }

    /// <summary>
    /// Remueve acentos y diacríticos.
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