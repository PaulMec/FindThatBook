using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace FindThatBook.Infrastructure.OpenLibrary.Models;

/// <summary>
/// Respuesta de /authors/{author_id}.json
/// </summary>
public class AuthorResponse
{
    [JsonPropertyName("key")]
    public string? Key { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("birth_date")]
    public string? BirthDate { get; set; }

    [JsonPropertyName("death_date")]
    public string? DeathDate { get; set; }

    [JsonPropertyName("bio")]
    public object? Bio { get; set; } // Puede ser string o { type, value }

    [JsonPropertyName("photos")]
    public List<int>? Photos { get; set; }

    [JsonPropertyName("alternate_names")]
    public List<string>? AlternateNames { get; set; }

    /// <summary>
    /// Extrae la biografía como string
    /// </summary>
    public string? GetBioText()
    {
        if (Bio == null)
            return null;

        if (Bio is string bioStr)
            return bioStr;

        if (Bio is System.Text.Json.JsonElement jsonElement)
        {
            if (jsonElement.ValueKind == System.Text.Json.JsonValueKind.String)
                return jsonElement.GetString();

            if (jsonElement.TryGetProperty("value", out var valueElement))
                return valueElement.GetString();
        }

        return Bio.ToString();
    }
}