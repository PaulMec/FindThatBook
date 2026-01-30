using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
    /// </summary>
    public bool HasAuthor(string authorName)
    {
        var normalizedName = authorName.ToLowerInvariant().Trim();

        if (PrimaryAuthor.GetNormalizedName().Contains(normalizedName))
            return true;

        return Contributors.Any(c => c.GetNormalizedName().Contains(normalizedName));
    }

    /// <summary>
    /// Obtiene el título normalizado para compararlo.
    /// </summary>
    public string GetNormalizedTitle() => Title.ToLowerInvariant().Trim();
}