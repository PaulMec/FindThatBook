using FindThatBook.Domain.Records;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Application.Interfaces;

public interface IBookRanker
{
    List<BookMatch> RankAndLimit(List<BookMatch> matches, int topN = 5);
}