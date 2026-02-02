using System.Text.Json;
using System.Net;
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

    // Cache simple para autores (evita llamadas repetidas)
    private readonly Dictionary<string, string> _authorNameCache = new();

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
                queryParams.Add($"title={WebUtility.UrlEncode(title)}");

            if (!string.IsNullOrWhiteSpace(author))
                queryParams.Add($"author={WebUtility.UrlEncode(author)}");

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
            // Normalizar workId (asegurarse de que tenga formato correcto)
            var normalizedId = NormalizeWorkId(workId);
            var url = $"{_options.BaseUrl}{normalizedId}.json";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Work not found: {WorkId}, Status: {Status}", workId, response.StatusCode);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var workResponse = JsonSerializer.Deserialize<WorkResponse>(json);

            if (workResponse == null || string.IsNullOrWhiteSpace(workResponse.Title))
            {
                _logger.LogWarning("Invalid work response for: {WorkId}", workId);
                return null;
            }

            // Obtener nombre del autor principal si hay referencias
            var primaryAuthorName = "Unknown Author";
            if (workResponse.Authors?.Any() == true)
            {
                var authorKey = workResponse.Authors.First().Author?.Key;
                if (!string.IsNullOrEmpty(authorKey))
                {
                    primaryAuthorName = await GetAuthorNameAsync(authorKey, cancellationToken) ?? "Unknown Author";
                }
            }

            // Obtener cover URL
            var coverUrl = workResponse.Covers?.FirstOrDefault() is int coverId and > 0
                ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"
                : null;

            // Parsear año de publicación
            int? firstPublishYear = null;
            if (!string.IsNullOrEmpty(workResponse.FirstPublishDate))
            {
                firstPublishYear = ExtractYear(workResponse.FirstPublishDate);
            }

            return new Book(
                title: workResponse.Title,
                primaryAuthor: new Author(primaryAuthorName),
                openLibraryWorkId: normalizedId,
                contributors: null,
                firstPublishYear: firstPublishYear,
                coverUrl: coverUrl,
                description: workResponse.GetDescriptionText()
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "HTTP error getting work details for {WorkId}", workId);
            return null;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON parsing error for work {WorkId}", workId);
            return null;
        }
    }

    /// <summary>
    /// Obtiene información de un autor por su ID
    /// </summary>
    public async Task<AuthorResponse?> GetAuthorAsync(
        string authorId,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting author details for: {AuthorId}", authorId);

        try
        {
            var normalizedId = authorId.StartsWith("/") ? authorId : $"/authors/{authorId}";
            var url = $"{_options.BaseUrl}{normalizedId}.json";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Author not found: {AuthorId}", authorId);
                return null;
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            return JsonSerializer.Deserialize<AuthorResponse>(json);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting author details for {AuthorId}", authorId);
            return null;
        }
    }

    /// <summary>
    /// Obtiene las obras de un autor por su ID usando /authors/{id}/works.json
    /// </summary>
    public async Task<List<Book>> GetAuthorWorksByIdAsync(
        string authorId,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting works for author ID: {AuthorId}", authorId);

        try
        {
            var normalizedId = authorId.StartsWith("/") ? authorId : $"/authors/{authorId}";
            var url = $"{_options.BaseUrl}{normalizedId}/works.json?limit={limit}";

            var response = await _httpClient.GetAsync(url, cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Author works not found: {AuthorId}", authorId);
                return new List<Book>();
            }

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var worksResponse = JsonSerializer.Deserialize<AuthorWorksResponse>(json);

            if (worksResponse?.Entries == null || !worksResponse.Entries.Any())
            {
                return new List<Book>();
            }

            // Obtener nombre del autor
            var authorName = await GetAuthorNameAsync(normalizedId, cancellationToken) ?? "Unknown Author";

            var books = worksResponse.Entries
                .Where(e => !string.IsNullOrWhiteSpace(e.Title) && !string.IsNullOrWhiteSpace(e.Key))
                .Select(entry => MapAuthorWorkToBook(entry, authorName))
                .Where(book => book != null)
                .Take(limit)
                .ToList();

            _logger.LogInformation("Found {Count} works for author {AuthorId}", books.Count, authorId);

            return books!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting works for author {AuthorId}", authorId);
            return new List<Book>();
        }
    }

    public async Task<List<Book>> GetAuthorWorksAsync(
        string authorName,
        int limit = 10,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Getting works by author name: {Author}", authorName);

        // Usar el endpoint de búsqueda por nombre de autor
        return await SearchBooksAsync(title: null, author: authorName, limit, cancellationToken);
    }

    #region Private Helper Methods

    private async Task<string?> GetAuthorNameAsync(string authorKey, CancellationToken cancellationToken)
    {
        // Verificar cache primero
        if (_authorNameCache.TryGetValue(authorKey, out var cachedName))
        {
            return cachedName;
        }

        try
        {
            var author = await GetAuthorAsync(authorKey, cancellationToken);
            if (author?.Name != null)
            {
                _authorNameCache[authorKey] = author.Name;
                return author.Name;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to get author name for {AuthorKey}", authorKey);
        }

        return null;
    }

    private static string NormalizeWorkId(string workId)
    {
        if (workId.StartsWith("/works/"))
            return workId;

        if (workId.StartsWith("/"))
        {
            // Elimine cualquier barra inicial y trátelo como un ID sin formato.
            var bareId = workId.TrimStart('/');
            return $"/works/{bareId}";
        }

        return $"/works/{workId}";
    }

    private static int? ExtractYear(string dateString)
    {
        if (string.IsNullOrWhiteSpace(dateString))
            return null;

        // Intentar extraer un año de 4 dígitos
        var match = System.Text.RegularExpressions.Regex.Match(dateString, @"\b(\d{4})\b");
        if (match.Success && int.TryParse(match.Groups[1].Value, out var year))
        {
            return year;
        }

        return null;
    }

    private Book? MapToBook(SearchDoc doc)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(doc.Title) || doc.AuthorName?.Any() != true)
                return null;

            var authorNames = doc.AuthorName
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .ToList();

            if (authorNames.Count == 0)
                return null;

            var primaryAuthor = new Author(authorNames[0]);

            if (string.IsNullOrWhiteSpace(doc.Key))
            {
                _logger.LogWarning("SearchDoc missing key for title: {Title}", doc.Title);
                return null;
            }
            var workId = doc.Key;

            var coverUrl = doc.CoverId.HasValue
                ? $"https://covers.openlibrary.org/b/id/{doc.CoverId}-L.jpg"
                : null;

            return new Book(
                title: doc.Title,
                primaryAuthor: primaryAuthor,
                openLibraryWorkId: workId,
                contributors: authorNames.Skip(1).Select(name => new Author(name)).ToList(),
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

    private Book? MapAuthorWorkToBook(AuthorWorkEntry entry, string authorName)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(entry.Title) || string.IsNullOrWhiteSpace(entry.Key))
                return null;

            var coverUrl = entry.Covers?.FirstOrDefault() is int coverId and > 0
                ? $"https://covers.openlibrary.org/b/id/{coverId}-L.jpg"
                : null;

            int? firstPublishYear = null;
            if (!string.IsNullOrEmpty(entry.FirstPublishDate))
            {
                firstPublishYear = ExtractYear(entry.FirstPublishDate);
            }

            return new Book(
                title: entry.Title,
                primaryAuthor: new Author(authorName),
                openLibraryWorkId: entry.Key,
                contributors: null,
                firstPublishYear: firstPublishYear,
                coverUrl: coverUrl,
                description: null
            );
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to map AuthorWorkEntry to Book: {Title}", entry.Title);
            return null;
        }
    }

    #endregion
}