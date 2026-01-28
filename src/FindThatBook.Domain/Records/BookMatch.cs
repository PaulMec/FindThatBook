using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Enums;

namespace FindThatBook.Domain.Records;

/// <summary>
/// Representa un libro que coincidió con la consulta de búsqueda con su nivel de confianza y explicación.
/// </summary>
public sealed record BookMatch
{
    public Book Book { get; init; }
    public MatchStrength Strength { get; init; }
    public string Explanation { get; init; }
    public double Score { get; init; }

    public BookMatch(Book book, MatchStrength strength, string explanation, double score = 0)
    {
        Book = book ?? throw new ArgumentNullException(nameof(book));

        if (string.IsNullOrWhiteSpace(explanation))
            throw new ArgumentException("La explicación no puede estar vacía.", nameof(explanation));

        Strength = strength;
        Explanation = explanation.Trim();
        Score = score;
    }

    /// <summary>
    /// Crea coincidencia de libro para el título exacto y el autor principal.
    /// </summary>
    public static BookMatch CreateStrongest(Book book, string matchedAuthor)
    {
        return new BookMatch(
            book,
            MatchStrength.Strongest,
            $"Coincidencia exacta del título; {matchedAuthor} es el autor principal.",
            score: 1.0
        );
    }

    /// <summary>
    /// Crea un BookMatch para el título exacto con coincidencia de colaborador.
    /// </summary>
    public static BookMatch CreateStrong(Book book, string matchedAuthor, string role)
    {
        return new BookMatch(
            book,
            MatchStrength.Strong,
            $"Coincidencia exacta del título; {matchedAuthor} aparece como {role}",
            score: 0.8
        );
    }

    /// <summary>
    /// Crea una coincidencia de libros para coincidencias de títulos aproximadas.
    /// </summary>
    public static BookMatch CreateMedium(Book book, string matchedTitle, double similarity)
    {
        return new BookMatch(
            book,
            MatchStrength.Medium,
            $"Título similar '{matchedTitle}'; coincidencia del autor (similitud: {similarity:P0})",
            score: similarity
        );
    }

    /// <summary>
    /// Crea un BookMatch para el autor como alternativa exclusiva.
    /// </summary>
    public static BookMatch CreateWeak(Book book, string authorName)
    {
        return new BookMatch(
            book,
            MatchStrength.Weak,
            $"Sin título; muestra las mejores obras de {authorName}",
            score: 0.5
        );
    }

    /// <summary>
    /// Crea una coincidencia de libro para la coincidencia de palabras clave.
    /// </summary>
    public static BookMatch CreateVeryWeak(Book book, IEnumerable<string> matchedKeywords)
    {
        var keywords = string.Join(", ", matchedKeywords);
        return new BookMatch(
            book,
            MatchStrength.VeryWeak,
            $"Coincidencia de palabras clave: {keywords} encontradas en el título/descripción",
            score: 0.3
        );
    }
}