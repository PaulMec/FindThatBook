using FindThatBook.Application.DTOs;
using FindThatBook.Application.UseCases;
using Microsoft.AspNetCore.Mvc;

namespace FindThatBook.Api.Controllers;

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

    [HttpPost("search")]
    [ProducesResponseType(typeof(SearchBooksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
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
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing search request");
            return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
            {
                Title = "Internal Server Error",
                Detail = "An error occurred while processing your request. Please try again later.",
                Status = StatusCodes.Status500InternalServerError
            });
        }
    }

    [HttpGet("health")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Health()
    {
        return Ok(new { status = "healthy", timestamp = DateTime.UtcNow });
    }
}