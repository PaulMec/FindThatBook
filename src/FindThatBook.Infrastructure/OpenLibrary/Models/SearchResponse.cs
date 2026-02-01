using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace FindThatBook.Infrastructure.OpenLibrary.Models;

public record SearchResponse
{
    [JsonPropertyName("numFound")]
    public int NumFound { get; init; }

    [JsonPropertyName("docs")]
    public List<SearchDoc> Docs { get; init; } = new();
}

public record SearchDoc
{
    [JsonPropertyName("key")]
    public string? Key { get; init; }

    [JsonPropertyName("title")]
    public string? Title { get; init; }

    [JsonPropertyName("author_name")]
    public List<string>? AuthorName { get; init; }

    [JsonPropertyName("first_publish_year")]
    public int? FirstPublishYear { get; init; }

    [JsonPropertyName("cover_i")]
    public long? CoverId { get; init; }

    [JsonPropertyName("isbn")]
    public List<string>? Isbn { get; init; }
}