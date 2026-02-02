using System;
using System.Globalization;
using System.Text;

namespace FindThatBook.Domain.ValueObjects;

/// <summary>
/// Objeto de valor representa autor de un libro.
/// Inmutable y valida que el nombre no esté vacio.
/// </summary>
public sealed record Author
{
    public string Name { get; init; }
    public string? OpenLibraryId { get; init; }

    public Author(string name, string? openLibraryId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("El nombre del autor no puede estar vacío.", nameof(name));

        Name = name.Trim();
        OpenLibraryId = openLibraryId;
    }

    /// <summary>
    /// Normaliza nombre de autor para comparaciones.
    /// Convierte a minúsculas y elimina acentos/diacríticos.
    /// </summary>
    public string GetNormalizedName()
    {
        return RemoveDiacritics(Name.ToLowerInvariant().Trim());
    }

    /// <summary>
    /// Elimina acentos y diacríticos del texto.
    /// "García Márquez" → "garcia marquez"
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

    public override string ToString() => Name;
}