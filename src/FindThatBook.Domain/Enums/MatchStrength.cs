using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FindThatBook.Domain.Enums;

/// <summary>
/// Representa el nivel de certeza de una coincidencia de libros basada en la extracción y comparación de consultas.
/// </summary>
public enum MatchStrength
{
    /// <summary>
    /// Coincidencia exacta del título con confirmación del autor principal
    /// </summary>
    Strongest = 1,

    /// <summary>
    /// Coincidencia exacta del título con el autor como colaborador
    /// </summary>
    Strong = 2,

    /// <summary>
    /// Título similar (coincidencia aproximada) 
    /// </summary>
    Medium = 3,

    /// <summary>
    /// Coincide solo el autor, no se ha encontrado ningún título.
    /// </summary>
    Weak = 4,

    /// <summary>
    /// Coincidencia de palabras clave en el título o la descripción
    /// </summary>
    VeryWeak = 5
}