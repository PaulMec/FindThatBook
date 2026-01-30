using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.DTOs;

public record SearchBooksResponse
{
    public string Query { get; init; } = string.Empty;
    public AIExtractionResult Extraction { get; init; } = new();
    public List<BookResultDto> Results { get; init; } = new();
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