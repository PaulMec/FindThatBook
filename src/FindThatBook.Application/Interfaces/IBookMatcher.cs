using FindThatBook.Application.DTOs;
using FindThatBook.Domain.Entities;
using FindThatBook.Domain.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.Interfaces;

public interface IBookMatcher
{
    List<BookMatch> Match(AIExtractionResult extraction, List<Book> candidates);
}