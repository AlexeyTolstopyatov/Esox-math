using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Esox.Types;

namespace Esox.Services;

public class LaTeXFrac32Markup
{
    public string Name { get; init; }
    
    // [Obsolete] Предупреждение здесь глупо.
    #pragma warning disable
    public LaTeXFrac32Markup()
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
    public string MakeCases(Frac32[,] common, Frac32[] freed, [Optional]string otherName)
    {
        if (common.GetLength(0) != freed.Length)
            throw new ArgumentException("Количество уравнений и свободных членов не совпадает");
        
        List<string> equations = new();
    
        for (int row = 0; row < common.GetLength(0); row++)
        {
            // Извлекаем коэффициенты для текущего уравнения
            Frac32[] coefficients = new Frac32[common.GetLength(1)];
            for (int col = 0; col < common.GetLength(1); col++)
            {
                coefficients[col] = common[row, col];
            }
        
            equations.Add(MakeEquation(coefficients, freed[row]));
        }

        return (string.IsNullOrEmpty(otherName) ? Name : otherName) + @" = \cases{" + Environment.NewLine + 
               string.Join(@" \\" + Environment.NewLine, equations) + 
               Environment.NewLine + "}";
    }

    /// <summary>
    /// Создает уравнение из массивов коэффициентов
    /// </summary>
    /// <param name="coefficients">Основная матрица системы</param>
    /// <param name="constant">Столбец свободных коэффициентов</param>
    private string MakeEquation(Frac32[] coefficients, Frac32 constant)
    {
        const double epsilon = 1e-10;
        var parts = new List<string>();
        bool firstNonZero = true;

        for (int i = 0; i < coefficients.Length; i++)
        {
            Frac32 coefficient = coefficients[i];
            if (Math.Abs(coefficient.Enumerator / coefficient.Denominator) < epsilon) continue;

            // Форматирование знака
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

            // Форматирование значения
            string value = "";
            double absCoeff = Math.Abs(coefficient.Enumerator / coefficient.Denominator);
            if (Math.Abs(absCoeff - 1.0) > epsilon)
            {
                value = $"{FormatNumber(coefficient)}";
            }

            parts.Add($"{sign}{value}x_{i + 1}");
        }

        // Обработка нулевого уравнения
        if (parts.Count == 0)
        {
            return constant.ToString();
        }

        // Форматирование правой части
        string rhs = constant.ToString();
        return $"{string.Join("", parts)} = {rhs}";
    }

    /// <summary>
    /// Создает разметку расширенной матрицы
    /// из общей матрицы и свободных коэффициентов 
    /// </summary>
    /// <param name="common"></param>
    /// <param name="freed"></param>
    /// <param name="otherName"></param>
    /// <returns></returns>
    public string MakePMatrix(Frac32[,] common, Frac32[] freed, [Optional] string otherName)
    {
        // build matrix
        StringBuilder sb = new();
        sb.Append(string.IsNullOrEmpty(otherName) ? Name : otherName);
        sb.Append(@" = \pmatrix{");
        
        for (int i = 0; i < common.GetLength(0); i++)
        {
            List<string> row = new();
            for (int j = 0; j < common.GetLength(1); j++)
            {
                row.Add(FormatNumber(common[i, j]));
            }
            sb.Append(string.Join(" & ", row) + @"\\");
        }
        
        sb.Remove(sb.Length - 2, 2);
        sb.Append(@"} \\");
        return sb.ToString();
    }

    private string FormatNumber(Frac32 number)
    {
        return number.ToString();
    }
}