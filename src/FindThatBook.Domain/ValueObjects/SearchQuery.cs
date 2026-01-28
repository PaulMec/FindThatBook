using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Domain.ValueObjects;

/// <summary>
/// Objeto de valor que representa la consulta de búsqueda del usuario.
/// Encapsula la consulta inicial y normaliza para su procesamiento
/// </summary>
public sealed record SearchQuery
{
    public string OriginalQuery { get; init; }
    public string NormalizedQuery { get; init; }

    public SearchQuery(string query)
    {
        if (string.IsNullOrWhiteSpace(query))
            throw new ArgumentException("La consulta de búsqueda no puede estar vacía.", nameof(query));

        OriginalQuery = query;
        NormalizedQuery = Normalize(query);
    }

    private static string Normalize(string query)
    {
        // Minusculas, trim, eliminar espacios
        var normalized = query.ToLowerInvariant().Trim();

        // Reemplazar varios espacios con un solo espacio
        while (normalized.Contains("  "))
            normalized = normalized.Replace("  ", " ");

        return normalized;
    }

    public override string ToString() => OriginalQuery;
}