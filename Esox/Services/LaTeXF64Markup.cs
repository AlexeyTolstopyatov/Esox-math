using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;
using Esox.Types;

namespace Esox.Services;

public class LaTeXF64Markup : ILaTexMarkup
{
    #pragma warning disable
    /// <summary>
    /// Создает имя объекта и предоставляет API
    /// для записи LaTeX разметки.
    /// </summary>
    public LaTeXF64Markup()
    {
        Name = MakeName();
    }
    #pragma warning restore
    
    /// <summary>
    /// Возвращает имя объекта
    /// </summary>
    public string Name { get; init; }
    
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
    /// Создает систему линейных уравнений
    /// из массивов свободных коэффициентов
    /// и общей матрицы коэффициентов
    /// </summary>
    /// <param name="commonMatrix">Общая матрица</param>
    /// <param name="constants">Свободные коэффициенты</param>
    /// <param name="otherName">Другое наименование объекта</param>
    /// <returns>
    /// </returns>
    /// <exception cref="ArgumentException">
    /// Если количество уравнений и свободных
    /// коэффициентов не совпадает
    /// </exception>
    public string MakeCases2(double[,] commonMatrix, double[] constants, [Optional] string otherName)
    {
        if (commonMatrix.GetLength(0) != constants.Length)
            throw new ArgumentException("Количество уравнений и свободных членов не совпадает");
        
        List<string> equations = new();
    
        for (int row = 0; row < commonMatrix.GetLength(0); row++)
        {
            // Извлекаем коэффициенты для текущего уравнения
            double[] coefficients = new double[commonMatrix.GetLength(1)];
            for (int col = 0; col < commonMatrix.GetLength(1); col++)
            {
                coefficients[col] = commonMatrix[row, col];
            }
        
            equations.Add(MakeEquation(coefficients, constants[row]));
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
    private string MakeEquation(double[] coefficients, double constant)
    {
        const double epsilon = 1e-10;
        var parts = new List<string>();
        bool firstNonZero = true;

        for (int i = 0; i < coefficients.Length; i++)
        {
            double coeff = coefficients[i];
            if (Math.Abs(coeff) < epsilon) continue;

            // Форматирование знака
            string sign;
            if (!firstNonZero)
            {
                sign = coeff > 0 ? " + " : " - ";
            }
            else
            {
                sign = coeff > 0 ? "" : "-";
                firstNonZero = false;
            }

            // Форматирование значения
            string value = "";
            double absCoeff = Math.Abs(coeff);
            if (Math.Abs(absCoeff - 1.0) > epsilon)
            {
                value = $"{FormatNumber(absCoeff)}";
            }

            parts.Add($"{sign}{value}x_{i + 1}");
        }

        // Обработка нулевого уравнения
        if (parts.Count == 0)
        {
            return $"0 = {constant:0.##}";
        }

        // Форматирование правой части
        string rhs = Math.Abs(constant) < epsilon ? "0" : $"{constant:0.##}";
        return $"{string.Join("", parts)} = {rhs}";
    }
    
    public string MakeCases(double[,] common, double[] freed, [Optional] string otherName)
    {
        StringBuilder sb = new();
        
        sb.Append(string.IsNullOrEmpty(otherName) ? Name : otherName);
        sb.Append(@" = \cases{");
        
        for (int i = 0; i < freed.Length; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < common.GetLength(1); j++)
            {
                terms.Add(FormatCoefficient(common[i, j], j + 1));
            }
            sb.AppendLine($"  {string.Join(" + ", terms).Replace("+ -", "- ")} = {FormatNumber(freed[i])} \\\\");
        }

        sb.Append("}");
        return sb.ToString();
    }
    /// <summary>
    /// Создает разметку расширенной матрицы
    /// из общей матрицы и свободных коэффициентов 
    /// </summary>
    /// <param name="common"></param>
    /// <param name="freed"></param>
    /// <param name="otherName"></param>
    /// <returns></returns>
    public string MakePMatrix(double[,] common, double[] freed, [Optional] string otherName)
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
    /// <summary>
    /// Создает Расширенную матрицу системы и подпись
    /// </summary>
    /// <param name="text"></param>
    /// <param name="isBold"></param>
    /// <param name="extended"></param>
    /// <param name="otherName"></param>
    /// <returns></returns>
    public string MakeExtendedPMatrixWithText(string text, [Optional] bool isBold, double[,] extended, [Optional] string otherName)
    {
        StringBuilder sb = new();
        sb.Append((isBold) 
            ? $@"\textbf{{{text}}} \\" 
            : $@"\text{{{text}}} \\");
        
        sb.Append(string.IsNullOrEmpty(otherName) 
            ? Name 
            : otherName);
        sb.Append(@" = \pmatrix{");
        
        for (int i = 0; i < extended.GetLength(0); i++)
        {
            // "<=" --> "<"
            for (int j = 0; j < extended.GetLength(1); j++)
            {
                // if FracF64 -> new FracF64()
                FracF64 f = new(extended[i, j]);
                sb.Append(f.Denomerator == 1
                    ? f.Enumerator 
                    : @$"\frac{{{f.Enumerator}}}{{{f.Denomerator}}}");
                
                if (j < extended.GetLength(1)) sb.Append(" & ");
            }
            sb.Append(@" \\");
        }
        sb.Insert(sb.Length - 2, "");
        sb.Insert(sb.Length - 1, "");
        sb.AppendLine(@"} \\");

        return sb.ToString();
    }

    private string FormatNumber(double number)
    {
        return Math.Round(number).ToString(CultureInfo.InvariantCulture);
    }
    
    private string FormatCoefficient(double value, int index)
    {
        if (value == 0) 
            return "";
        string formatted = FormatNumber(value);
        return value switch
        {
            1 => $"x_{{{index}}}",
            -1 => $"-{{x_{index}}}",
            _ => $"{{{formatted}}}x_{{{index}}}"
        };
    }
}