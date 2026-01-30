using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.DTOs
{
    public record SearchBooksRequest
    {
        public string Query { get; init; } = string.Empty;
    }
}