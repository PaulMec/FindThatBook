using System.Text;
using System.Text.Json;
using FindThatBook.Application.DTOs;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Infrastructure.AI.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FindThatBook.Infrastructure.AI;

public class GeminiAIProvider : IAIFieldExtractor
{
    private readonly HttpClient _httpClient;
    private readonly GeminiOptions _options;
    private readonly ILogger<GeminiAIProvider> _logger;

    public GeminiAIProvider(
        HttpClient httpClient,
        IOptions<GeminiOptions> options,
        ILogger<GeminiAIProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<AIExtractionResult> ExtractFieldsAsync(
        string query,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Extracting fields from query using Gemini AI: {Query}", query);

        try
        {
            var prompt = BuildPrompt(query);
            var request = new GeminiRequest
            {
                Contents = new List<Content>
                {
                    new Content
                    {
                        Parts = new List<Part>
                        {
                            new Part { Text = prompt }
                        }
                    }
                },
                GenerationConfig = new GenerationConfig
                {
                    Temperature = 0.1,
                    ResponseMimeType = "application/json"
                }
            };

            var url = $"{_options.BaseUrl}/{_options.Model}:generateContent?key={_options.ApiKey}";
            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content, cancellationToken);
            response.EnsureSuccessStatusCode();

            var responseBody = await response.Content.ReadAsStringAsync(cancellationToken);

            _logger.LogDebug("Gemini API response: {Response}", responseBody);

            return ParseGeminiResponse(responseBody, query);
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Gemini API for query: {Query}", query);
            throw new AIExtractionException(query, "Failed to call Gemini API", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for Gemini response");
            throw new AIExtractionException(query, "Failed to parse Gemini response", ex);
        }
    }

    private string BuildPrompt(string query)
    {
        return $@"You are a book metadata extractor. Analyze this query and extract book information.

Query: ""{query}""

Extract the following fields (all optional):
- title: The book title (if present)
- author: The author name (if present)
- year: Publication year (if present, as integer)
- keywords: Any other relevant keywords (as array)

Return ONLY valid JSON in this exact format:
{{
  ""title"": ""extracted title or null"",
  ""author"": ""extracted author or null"",
  ""year"": 1234,
  ""keywords"": [""keyword1"", ""keyword2""]
}}

Rules:
- If a field is not present, use null (not empty string)
- Keywords should NOT include title or author
- Be conservative - only extract what you're confident about
- Return ONLY the JSON object, no explanations";
    }

    private AIExtractionResult ParseGeminiResponse(string responseBody, string originalQuery)
    {
        try
        {
            var jsonDoc = JsonDocument.Parse(responseBody);

            // Gemini envuelve la respuesta en: candidates[0].content.parts[0].text
            var candidates = jsonDoc.RootElement.GetProperty("candidates");
            var firstCandidate = candidates[0];
            var content = firstCandidate.GetProperty("content");
            var parts = content.GetProperty("parts");
            var text = parts[0].GetProperty("text").GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Gemini returned empty text");
                return new AIExtractionResult();
            }

            // Analiza el JSON interno (la extracción real)
            var extractionJson = JsonDocument.Parse(text);
            var root = extractionJson.RootElement;

            return new AIExtractionResult
            {
                Title = root.TryGetProperty("title", out var titleProp) && titleProp.ValueKind != JsonValueKind.Null
                    ? titleProp.GetString()
                    : null,
                Author = root.TryGetProperty("author", out var authorProp) && authorProp.ValueKind != JsonValueKind.Null
                    ? authorProp.GetString()
                    : null,
                Year = root.TryGetProperty("year", out var yearProp) && yearProp.ValueKind == JsonValueKind.Number
                    ? yearProp.GetInt32()
                    : null,
                Keywords = root.TryGetProperty("keywords", out var keywordsProp) && keywordsProp.ValueKind == JsonValueKind.Array
                    ? keywordsProp.EnumerateArray().Select(k => k.GetString() ?? "").Where(k => !string.IsNullOrWhiteSpace(k)).ToList()
                    : new List<string>()
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse Gemini response: {Response}", responseBody);
            throw new AIExtractionException(originalQuery, "Failed to parse AI response", responseBody);
        }
    }
}