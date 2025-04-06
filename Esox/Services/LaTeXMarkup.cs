using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Text;

namespace Esox.Services;

public class LaTeXMarkup : ILaTexMarkup
{
    /// <summary>
    /// Создает имя объекта и предоставляет API
    /// для записи LaTeX разметки.
    /// </summary>
    #pragma warning disable
    public LaTeXMarkup()
    {
        Name = MakeName();
    }
    #pragma warning restore
    
    public string Name { get; init; }
    [Obsolete("Calls once when class initializes")]
    public string MakeName()
    {
        return Convert
            .ToChar(Random.Shared.Next(0x41, 0x5A))
            .ToString();
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
    
    public string MakePMatrixWithText(string text, bool isBold)
    {
        throw new NotImplementedException();
    }

    private string FormatNumber(double number)
    {
        return number.ToString(CultureInfo.InvariantCulture);
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