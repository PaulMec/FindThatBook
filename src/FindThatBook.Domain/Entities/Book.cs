using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

using FindThatBook.Domain.ValueObjects;

namespace FindThatBook.Domain.Entities;

/// <summary>
/// Representa un libro de Open Library con sus metadatos.
/// </summary>
public sealed class Book
{
    public string Title { get; init; }
    public Author PrimaryAuthor { get; init; }
    public IReadOnlyList<Author> Contributors { get; init; }
    public int? FirstPublishYear { get; init; }
    public string OpenLibraryWorkId { get; init; }
    public string? CoverUrl { get; init; }
    public string? Description { get; init; }

    public Book(
        string title,
        Author primaryAuthor,
        string openLibraryWorkId,
        IReadOnlyList<Author>? contributors = null,
        int? firstPublishYear = null,
        string? coverUrl = null,
        string? description = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("El título del libro no puede estar vacío.", nameof(title));

        if (string.IsNullOrWhiteSpace(openLibraryWorkId))
            throw new ArgumentException("Se requiere el ID de trabajo de OpenLibrary.", nameof(openLibraryWorkId));

        Title = title.Trim();
        PrimaryAuthor = primaryAuthor ?? throw new ArgumentNullException(nameof(primaryAuthor));
        OpenLibraryWorkId = openLibraryWorkId;
        Contributors = contributors ?? Array.Empty<Author>();
        FirstPublishYear = firstPublishYear;
        CoverUrl = coverUrl;
        Description = description;
    }

    /// <summary>
    /// Obtiene la URL completa de OpenLibrary para esta obra.
    /// </summary>
    public string GetOpenLibraryUrl()
    {
        var workId = OpenLibraryWorkId.StartsWith("/")
            ? OpenLibraryWorkId
            : $"/works/{OpenLibraryWorkId}";

        return $"https://openlibrary.org{workId}";
    }

    /// <summary>
    /// Comprueba si el autor (por nombre) es el autor principal o colaborador.
    /// Soporta matching parcial por palabras individuales.
    /// </summary>
    public bool HasAuthor(string authorName)
    {
        if (string.IsNullOrWhiteSpace(authorName))
            return false;

        // Normalizamos la búsqueda igual que el autor (minúsculas + sin diacríticos).
        var normalizedSearch = RemoveDiacritics(authorName.ToLowerInvariant().Trim());

        if (string.IsNullOrWhiteSpace(normalizedSearch))
            return false;

        var searchWords = normalizedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        // Check primary author
        if (MatchesAuthorName(PrimaryAuthor.GetNormalizedName(), normalizedSearch, searchWords))
            return true;

        // Check contributors
        return Contributors.Any(c =>
            MatchesAuthorName(c.GetNormalizedName(), normalizedSearch, searchWords));
    }

    /// <summary>
    /// Verifica si el nombre del autor coincide con la búsqueda.
    /// Soporta: match exacto, contains, y match por palabras individuales.
    /// </summary>
    private bool MatchesAuthorName(string authorName, string searchFull, string[] searchWords)
    {
        if (string.IsNullOrWhiteSpace(authorName) || string.IsNullOrWhiteSpace(searchFull))
            return false;

        // 1. Match exacto o contains
        if (authorName.Contains(searchFull))
            return true;

        // 2. Match inverso (búsqueda contiene al autor) con umbral para evitar falsos positivos
        if (authorName.Length >= 4 && searchFull.Contains(authorName))
            return true;

        // 3. Match por palabras: si TODAS las palabras de búsqueda están en el nombre
        if (searchWords.Length > 1)
        {
            var allWordsMatch = searchWords.All(word => authorName.Contains(word));
            if (allWordsMatch)
                return true;
        }

        // 4. Match por apellido: si alguna palabra de búsqueda (>3 chars) está en el nombre
        var significantWords = searchWords.Where(w => w.Length > 3);
        if (significantWords.Any(word => authorName.Contains(word)))
            return true;

        return false;
    }

    /// <summary>
    /// Obtiene el título normalizado para compararlo.
    /// Minúsculas + sin diacríticos (para buscar "anos" vs "años").
    /// </summary>
    public string GetNormalizedTitle()
        => RemoveDiacritics(Title.ToLowerInvariant().Trim());

    /// <summary>
    /// Elimina acentos y diacríticos del texto para comparaciones.
    /// </summary>
    private static string RemoveDiacritics(string text)
    {
        var normalized = text.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder();

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                builder.Append(c);
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}