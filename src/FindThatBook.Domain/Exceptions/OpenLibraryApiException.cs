using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Domain.Exceptions;

/// <summary>
/// Exception thrown when Open Library API fails
/// </summary>
public sealed class OpenLibraryApiException : BookSearchException
{
    public string? Endpoint { get; }
    public int? StatusCode { get; }

    public OpenLibraryApiException(string message)
        : base(message)
    {
    }

    public OpenLibraryApiException(string message, string endpoint, int statusCode)
        : base(message)
    {
        Endpoint = endpoint;
        StatusCode = statusCode;
    }

    public OpenLibraryApiException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}