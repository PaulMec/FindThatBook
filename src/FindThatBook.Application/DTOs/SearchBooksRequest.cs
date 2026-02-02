using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.DTOs
{
    public record SearchBooksRequest
    {
        [Required(ErrorMessage = "Query is required")]
        [StringLength(500, MinimumLength = 1, ErrorMessage = "Query must be between 1 and 500 characters")]
        public string Query { get; init; } = string.Empty;
    }
}