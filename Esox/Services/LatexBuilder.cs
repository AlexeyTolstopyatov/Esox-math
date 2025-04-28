using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Esox.Types;

namespace Esox.Services;

public class LatexBuilder
{
    public string Name { get; init; }
    
    // [Obsolete] Предупреждение здесь глупо.
    #pragma warning disable
    public LatexBuilder()
    {
        Name = MakeName();
    }
    #pragma warning restore
    /// <summary>
    /// Создает наименование объекта
    /// </summary>
    /// <returns></returns>
    [Obsolete("Calls once when class initializes")]
    public string MakeName()
    {
        return Convert
            .ToChar(Random.Shared.Next(0x41, 0x5A))
            .ToString();
    }
    /// <summary>
    /// Создает LaTeX разметку с указанным текстом
    /// </summary>
    /// <param name="text">Текст</param>
    /// <param name="isBold">Сделать Жирным</param>
    /// <returns></returns>
    public string MakeText(string text, [Optional] bool isBold)
    {
        string macro = isBold
            ? @"\textbf{"
            : @"\text{";
        macro += text;
        macro += @"} \\";
        
        return macro;
    }
    /// <summary>
    /// Создает разметку LaTeX для системы
    /// линейных уравнений
    /// </summary>
    /// <param name="common">Матрица коэффициентов</param>
    /// <param name="freed">Свободный столбец</param>
    /// <param name="otherName">Имя объекта</param>
    /// <exception cref="ArgumentException">Количество уравнений и свободных членов не совпадает</exception>
    public string MakeCases(Frac32[,] common, Frac32[] freed, [Optional] string otherName)
    {
        if (common.GetLength(0) != freed.Length)
            throw new ArgumentException("Количество уравнений и свободных членов не совпадает");

        List<string> equations = new();

        for (int row = 0; row < common.GetLength(0); row++)
        {
            Frac32[] coefficients = new Frac32[common.GetLength(1)];
            for (int col = 0; col < common.GetLength(1); col++)
            {
                coefficients[col] = common[row, col];
            }
            
            equations.Add(MakeEquation(coefficients, freed[row]));
        }
        
        return (string.IsNullOrEmpty(otherName) ? Name : otherName) + @" = \cases{" + Environment.NewLine +
               string.Join(@" \\" + Environment.NewLine, equations) +
               Environment.NewLine + @"}";
    }

    private string MakeEquation(Frac32[] coefficients, Frac32 constant)
    {
        var parts = new List<string>();
        bool firstNonZero = true;

        for (int i = 0; i < coefficients.Length; i++)
        {
            Frac32 coefficient = coefficients[i];
            
            // Исправлено: точная проверка на ноль для дробей
            if (coefficient == Frac32.Zero) 
                continue;

            // Определение знака
            string sign;
            if (!firstNonZero)
            {
                sign = coefficient > Frac32.Zero ? " + " : " - ";
            }
            else
            {
                sign = coefficient > Frac32.Zero ? "" : "-";
                firstNonZero = false;
            }

            // Форматирование значения !
            
            Frac32 absolute = Frac32.Abs(coefficient);
            string value = "";
            
            if (absolute != Frac32.Positive)
            {
                value = FormatFraction(absolute);
            }

            parts.Add($"{sign}{value}x_{i + 1}");
        }
        
        if (parts.Count == 0)
        {
            return $"0 = {FormatFraction(constant)}";
        }
        
        // Форматирование правой части
        return $"{string.Join("", parts)} = {FormatFraction(constant)}";
    }
    

    private string FormatFraction(Frac32 frac)
    {
        if (frac.Denominator == -1)
            return $"-{frac.Enumerator}";
        
        return frac.Denominator == 1 ? $"{frac.Enumerator}" :
            $@"\frac{{{frac.Enumerator}}}{{{frac.Denominator}}}";
    }

    /// <summary>
    /// Создает разметку расширенной матрицы
    /// из общей матрицы и свободных коэффициентов 
    /// </summary>
    /// <param name="common">Основная матрица коэффициентов</param>
    /// <param name="freed">Свободный вектор-столбец</param>
    /// <param name="otherName">Имя объекта</param>
    public string MakePMatrix(Frac32[,] common, Frac32[] freed, [Optional] string otherName)
    {
        // build matrix
        StringBuilder sb = new();
        sb.Append(string.IsNullOrEmpty(otherName) ? Name : otherName);
        sb.Append(@" = \begin{pmatrix}");
        
        for (int i = 0; i < common.GetLength(0); i++)
        {
            List<string> row = new();
            for (int j = 0; j < common.GetLength(1); j++)
            {
                row.Add(FormatFraction(common[i, j]));
            }
            sb.Append(string.Join(" & ", row) + @" \\ ");
        }

        sb.Insert(sb.Length - 3, "");
        sb.Insert(sb.Length - 2, "");
        sb.Insert(sb.Length - 1, "");
        sb.Append(@"\end{pmatrix}");

        sb.Append(@"\begin{pmatrix}");
        foreach (Frac32 frac in freed)
        {
            sb.Append(frac + @"\\");
        }

        sb.Append(@"\end{pmatrix}");
        
        return sb.ToString();
    }
    /// <summary>
    /// Создает разметку расширенной матрицы
    /// из общей матрицы и свободных коэффициентов 
    /// </summary>
    /// <param name="matrix">матрица коэффициентов</param>
    /// <param name="otherName">Имя объекта</param>
    public string MakePMatrix(Frac32[,] matrix, [Optional] string otherName)
    {
        // build matrix
        StringBuilder sb = new();
        sb.Append(string.IsNullOrEmpty(otherName) ? Name : otherName);
        sb.Append(@" = \begin{pmatrix}");
        
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            List<string> row = new();
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                row.Add(FormatFraction(matrix[i, j]));
            }
            sb.Append(string.Join(" & ", row) + @" \\ ");
        }

        sb.Insert(sb.Length - 3, "");
        sb.Insert(sb.Length - 2, "");
        sb.Insert(sb.Length - 1, "");
        sb.Append(@"\end{pmatrix}");
        
        return sb.ToString();
    }

    /// <summary>
    /// Создает разметку Вектор-столбца
    /// </summary>
    /// <param name="data">Данные вектор-столбца</param>
    public string MakePVectorColumn(Frac32[] data)
    {
        string vector = data.Aggregate(@"\begin{pmatrix}", 
            (current, t) => current + (t + @" \\"));

        vector += @"\end{pmatrix}";
        return vector;
    }
    /// <summary>
    /// Создает разметку Вектор-строки
    /// </summary>
    /// <param name="data">Данные вектор-строки</param>
    public string MakePVectorRow(Frac32[] data)
    {
        string vector = data.Aggregate("[", 
            (current, t) => current + (t + "; "));

        vector += "]";
        return vector;
    }
}