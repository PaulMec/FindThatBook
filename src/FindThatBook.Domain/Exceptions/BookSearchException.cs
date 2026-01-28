using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Domain.Exceptions;

/// <summary>
///Crea una coincidencia de libro para la coincidencia de palabras clave.
/// </summary>
public class BookSearchException : Exception
{
    public BookSearchException(string message) : base(message)
    {
    }

    public BookSearchException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}