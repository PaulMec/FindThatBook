using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            throw new ArgumentException("El nombre del autor  no puede estar vacío.", nameof(name));

        Name = name.Trim();
        OpenLibraryId = openLibraryId;
    }

    /// <summary>
    /// Normaliza nombre de autor para comparaciones(minusculas, sin espacios extras).
    /// </summary>
    public string GetNormalizedName() => Name.ToLowerInvariant().Trim();

    public override string ToString() => Name;
}