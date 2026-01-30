using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindThatBook.Infrastructure.AI.Models;

public record GeminiRequest
{
    [JsonPropertyName("contents")]
    public List<Content> Contents { get; init; } = new();

    [JsonPropertyName("generationConfig")]
    public GenerationConfig? GenerationConfig { get; init; }
}

public record Content
{
    [JsonPropertyName("parts")]
    public List<Part> Parts { get; init; } = new();
}

public record Part
{
    [JsonPropertyName("text")]
    public string Text { get; init; } = string.Empty;
}

public record GenerationConfig
{
    [JsonPropertyName("temperature")]
    public double Temperature { get; init; } = 0.1;

    [JsonPropertyName("responseMimeType")]
    public string ResponseMimeType { get; init; } = "application/json";
}
