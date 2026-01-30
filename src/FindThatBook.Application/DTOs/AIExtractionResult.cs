using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.DTOs;

public record AIExtractionResult
{
    public string? Title { get; init; }
    public string? Author { get; init; }
    public int? Year { get; init; }
    public List<string> Keywords { get; init; } = new();

    public bool HasTitle => !string.IsNullOrWhiteSpace(Title);
    public bool HasAuthor => !string.IsNullOrWhiteSpace(Author);
    public bool HasAnyField => HasTitle || HasAuthor || Keywords.Any();
}