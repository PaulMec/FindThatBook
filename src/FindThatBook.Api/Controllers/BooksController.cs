using FindThatBook.Application.DTOs;
using FindThatBook.Application.UseCases;
using FindThatBook.Domain.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.Api.Controllers;

/// <summary>
/// Controlador de libros: expone búsqueda y health.
/// Maneja códigos 200/400/500 y registra eventos clave.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class BooksController : ControllerBase
{
    private readonly SearchBooksUseCase _searchBooksUseCase;
    private readonly ILogger<BooksController> _logger;

    public BooksController(
        SearchBooksUseCase searchBooksUseCase,
        ILogger<BooksController> logger)
    {
        _searchBooksUseCase = searchBooksUseCase;
        _logger = logger;
    }

    /// <summary>
    /// Ejecuta la búsqueda y retorna hasta 5 resultados con explicación.
    /// </summary>
    /// <param name="request">Texto libre (Query) a analizar.</param>
    /// <param name="cancellationToken">Token de cancelación opcional.</param>
    /// <returns>Respuesta con extracción de IA y lista de resultados.</returns>
    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchBooksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status429TooManyRequests)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status503ServiceUnavailable)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<SearchBooksResponse>> Search(
        [FromBody] SearchBooksRequest request,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Received search request");

        try
        {
            var response = await _searchBooksUseCase.ExecuteAsync(request, cancellationToken);

            _logger.LogInformation("Search completed. Found {Count} results", response.Results.Count);

            return Ok(response);
        }
        catch (ArgumentNullException ex)
        {
            _logger.LogWarning(ex, "Null request received");
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = "Request body is required.",
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid request: {Message}", ex.Message);
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid Request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
        {
            _logger.LogWarning(ex, "Rate limit exceeded");
            return StatusCode(StatusCodes.Status429TooManyRequests, new ProblemDetails
            {
                Title = "Rate Limit Exceeded",
                Detail = "Too many requests. Please wait a moment and try again.",
                Status = StatusCodes.Status429TooManyRequests
            });
        }
        catch (AIExtractionException ex)
        {
            _logger.LogWarning(ex, "AI extraction failed");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "AI Service Unavailable",
                Detail = "The AI service is temporarily unavailable. Please try again.",
                Status = StatusCodes.Status503ServiceUnavailable
            });
        }
        catch (OpenLibraryApiException ex)
        {
            _logger.LogWarning(ex, "Open Library API error");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new ProblemDetails
            {
                Title = "Book Service Unavailable",
                Detail = "The book search service is temporarily unavailable. Please try again.",
                Status = StatusCodes.Status503ServiceUnavailable
            });
        }
        catch (TaskCanceledException ex) when (ex.CancellationToken.IsCancellationRequested)
        {
            _logger.LogInformation("Request was cancelled");
            return StatusCode(499, new ProblemDetails
            {
                Title = "Request Cancelled",
                Detail = "The request was cancelled.",
                Status = 499
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing search request");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An unexpected error occurred. Please try again later.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    /// <summary>
    /// Verificación rápida del estado de la API.
    /// </summary>
    /// <returns>Objeto con estado healthy y timestamp UTC.</returns>
    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}