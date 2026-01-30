using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json;
using System.Web;
using FindThatBook.Application.Interfaces;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Exceptions;
using FindThatBook.Domain.ValueObjects;
using FindThatBook.Infrastructure.OpenLibrary.Models;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FindThatBook.Infrastructure.OpenLibrary;

public class OpenLibraryClient : IOpenLibraryClient
{
    private readonly HttpClient _httpClient;
    private readonly OpenLibraryOptions _options;
    private readonly ILogger<OpenLibraryClient> _logger;

    public OpenLibraryClient(
        HttpClient httpClient,
        IOptions<OpenLibraryOptions> options,
        ILogger<OpenLibraryClient> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<List<Book>> SearchBooksAsync(
        string? title,
        string? author,
        int limit = 20,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Searching Open Library - Title: {Title}, Author: {Author}, Limit: {Limit}",
            title ?? "null", author ?? "null", limit);

        try
        {
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(title))
                queryParams.Add($"title={HttpUtility.UrlEncode(title)}");

            if (!string.IsNullOrWhiteSpace(author))
                queryParams.Add($"author={HttpUtility.UrlEncode(author)}");

            if (!queryParams.Any())
            {
                _logger.LogWarning("No search parameters provided");
                return new List<Book>();
            }

            queryParams.Add($"limit={limit}");

            var url = $"{_options.BaseUrl}/search.json?{string.Join("&", queryParams)}";

            _logger.LogDebug("Calling Open Library API: {Url}", url);

            var response = await _httpClient.GetAsync(url, cancellationToken);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var searchResponse = JsonSerializer.Deserialize<SearchResponse>(json);

            if (searchResponse?.Docs == null || !searchResponse.Docs.Any())
            {
                _logger.LogInformation("No results found from Open Library");
                return new List<Book>();
            }

            var books = searchResponse.Docs
                .Where(doc => !string.IsNullOrWhiteSpace(doc.Title) && doc.AuthorName?.Any() == true)
                .Select(MapToBook)
                .Where(book => book != null)
                .ToList();

            _logger.LogInformation("Mapped {Count} books from Open Library response", books.Count);

            return books!;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error calling Open Library API");
            throw new OpenLibraryApiException("Failed to search Open Library", ex);
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for Open Library response");
            throw new OpenLibraryApiException("Failed to parse Open Library response", ex);
        }
    }
    public async Task<Book?> GetWorkDetailsAsync(
    string workId,
    CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting work details for: {WorkId}", workId);

        try
        {
            // Normalizar workId (asegurarse de que comience con /)
            var normalizedId = workId.StartsWith("/") ? workId : $"/works/{workId}";
            var url = $"{_options.BaseUrl}{normalizedId}.json";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Work not found: {WorkId}", workId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            // TODO: Implementar el análisis completo de los detalles del trabajo si es necesario.

            return null; // Por ahora, devuelve null (se puede ampliar más adelante).
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting work details for {WorkId}", workId);
            return null;
        }
    }

    public async Task<List<Book>> GetAuthorWorksAsync(
        string authorName,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting works by author: {Author}", authorName);

        // Para MVP, podemos utilizar el mismo punto final de búsqueda con solo autor.
        return await SearchBooksAsync(title: null, author: authorName, limit, cancellationToken);
    }

    private Book? MapToBook(SearchDoc doc)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(doc.Title) || doc.AuthorName?.Any() != true)
                return null;

            var primaryAuthorName = doc.AuthorName.First();
            var primaryAuthor = new Author(primaryAuthorName);

            // Open Library uses /works/OL123W format
            var workId = doc.Key ?? $"/works/UNKNOWN";

            var coverUrl = doc.CoverId.HasValue
                ? $"https://covers.openlibrary.org/b/id/{doc.CoverId}-L.jpg"
                : null;

            return new Book(
                title: doc.Title,
                primaryAuthor: primaryAuthor,
                openLibraryWorkId: workId,
                contributors: doc.AuthorName.Skip(1).Select(name => new Author(name)).ToList(),
                firstPublishYear: doc.FirstPublishYear,
                coverUrl: coverUrl,
                description: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to map SearchDoc to Book: {Title}", doc.Title);
            return null;
        }
    }
}