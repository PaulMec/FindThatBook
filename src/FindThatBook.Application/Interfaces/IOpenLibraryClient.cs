using FindThatBook.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.Interfaces;

public interface IOpenLibraryClient
{
    Task<List<Book>> SearchBooksAsync(
        string? title,
        string? author,
        int limit = 20,
        CancellationToken cancellationToken = default);

    Task<Book?> GetWorkDetailsAsync(
        string workId,
        CancellationToken cancellationToken = default);

    Task<List<Book>> GetAuthorWorksAsync(
        string authorName,
        int limit = 10,
        CancellationToken cancellationToken = default);
}