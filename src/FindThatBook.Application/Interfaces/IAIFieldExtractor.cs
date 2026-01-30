using FindThatBook.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.Interfaces;
    public interface IAIFieldExtractor
{
    Task<AIExtractionResult> ExtractFieldsAsync(string query, CancellationToken cancellationToken = default);
}
