using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindThatBook.Infrastructure.OpenLibrary.Models;

/// <summary>
/// Respuesta de /works/{work_id}.json
/// </summary>
public class WorkResponse
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("title")]
    public string? Title { get; set; }

    [JsonPropertyName("description")]
    public object? Description { get; set; } // Puede ser string o { type, value }

    [JsonPropertyName("covers")]
    public List<int>? Covers { get; set; }

    [JsonPropertyName("authors")]
    public List<WorkAuthorReference>? Authors { get; set; }

    [JsonPropertyName("first_publish_date")]
    public string? FirstPublishDate { get; set; }

    [JsonPropertyName("subjects")]
    public List<string>? Subjects { get; set; }

    /// <summary>
    /// Extrae la descripción como string (puede venir como objeto o string directo)
    /// </summary>
    public string? GetDescriptionText()
    {
        if (Description == null)
            return null;

        if (Description is string descStr)
            return descStr;

        // Si es un objeto JSON con { "type": "/type/text", "value": "..." }
        if (Description is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                return jsonElement.GetString();

            if (jsonElement.TryGetProperty("value", out var valueElement))
                return valueElement.GetString();
        }

        return Description.ToString();
    }
}

public class WorkAuthorReference
{
    [JsonPropertyName("author")]
    public AuthorKey? Author { get; set; }

    [JsonPropertyName("type")]
    public TypeReference? Type { get; set; }
}

public class AuthorKey
{
    [JsonPropertyName("key")]
    public string? Key { get; set; } // e.g., "/authors/OL23919A"
}

public class TypeReference
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }
}