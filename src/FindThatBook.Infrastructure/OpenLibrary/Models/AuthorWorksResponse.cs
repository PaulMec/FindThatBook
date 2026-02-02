using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindThatBook.Infrastructure.OpenLibrary.Models;

/// <summary>
/// Respuesta de /authors/{author_id}/works.json
/// </summary>
public class AuthorWorksResponse
{
    [JsonPropertyName("size")]
    public int Size { get; set; }

    [JsonPropertyName("entries")]
    public List<AuthorWorkEntry>? Entries { get; set; }
}

public class AuthorWorkEntry
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }

    [JsonPropertyName("first_publish_date")]
    public string? FirstPublishDate { get; set; }

    [JsonPropertyName("authors")]
    public List<WorkAuthorReference>? Authors { get; set; }
}