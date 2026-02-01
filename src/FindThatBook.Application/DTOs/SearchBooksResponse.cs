using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.DTOs;

public class SearchBooksResponse
{
    public string Query { get; set; } = string.Empty;
    public AIExtractionResult Extraction { get; set; } = new();
    public List<BookResultDto> Results { get; set; } = new();

    /// <summary>
    /// Mensaje opcional para el usuario (ej: "No results found", "Rate limit exceeded")
    /// </summary>
    public string? Message { get; set; }
}

public record BookResultDto
{
    public string Title { get; init; } = string.Empty;
    public string Author { get; init; } = string.Empty;
    public int? FirstPublishYear { get; init; }
    public string OpenLibraryId { get; init; } = string.Empty;
    public string OpenLibraryUrl { get; init; } = string.Empty;
    public string? CoverUrl { get; init; }
    public string Explanation { get; init; } = string.Empty;
}