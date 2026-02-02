using System.Globalization;
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
    /// Comprueba autor (por nombre) es el autor principal o colaborador.
    /// Normaliza acentos y caracteres especiales para mejor matching.
    /// </summary>
    public bool HasAuthor(string authorName)
    {
        // Early guard para evitar match con string vacío
        if (string.IsNullOrWhiteSpace(authorName))
            return false;

        var normalizedSearch = RemoveDiacritics(authorName.ToLowerInvariant()).Trim();

        // Si después de normalizar queda vacío, retornar false
        if (string.IsNullOrWhiteSpace(normalizedSearch))
            return false;

        var normalizedPrimary = RemoveDiacritics(PrimaryAuthor.GetNormalizedName());
        if (normalizedPrimary.Contains(normalizedSearch) ||
            normalizedSearch.Contains(normalizedPrimary))
            return true;

        // También verificar si las palabras del nombre buscado están en el autor
        var searchWords = normalizedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (searchWords.Length > 1)
        {
            var matchedWords = searchWords.Count(word => normalizedPrimary.Contains(word));
            if (matchedWords >= 2) // Al menos 2 palabras coinciden
                return true;
        }

        return Contributors.Any(c =>
        {
            var normalizedContributor = RemoveDiacritics(c.GetNormalizedName());
            return normalizedContributor.Contains(normalizedSearch) ||
                   normalizedSearch.Contains(normalizedContributor);
        });
    }

    /// <summary>
    /// Obtiene el título normalizado para compararlo.
    /// </summary>
    public string GetNormalizedTitle() => Title.ToLowerInvariant().Trim();

    /// <summary>
    /// Remueve acentos y diacríticos de un string.
    /// Ej: "García" -> "garcia", "Márquez" -> "marquez"
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