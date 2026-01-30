using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Exception thrown when AI fails to extract fields from query
/// </summary>
public sealed class AIExtractionException : BookSearchException
{
    public string Query { get; }
    public string? AIResponse { get; }

    public AIExtractionException(string query, string message)
        : base(message)
    {
        Query = query;
    }

    public AIExtractionException(string query, string message, string aiResponse)
        : base(message)
    {
        Query = query;
        AIResponse = aiResponse;
    }

    public AIExtractionException(string query, string message, Exception innerException)
        : base(message, innerException)
    {
        Query = query;
    }
}