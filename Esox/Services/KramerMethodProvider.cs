using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using Esox.Models;

namespace Esox.Services;

public class KramerMethodProvider : IProviderService
{
    public KramerMethodProvider(int ordinal, bool homo)
    {
        int[] constants = new int[ordinal];
        
        GenerateKramerModel(GenerateMatrix(ordinal), 
            homo ? constants 
                 : GenerateVector(ordinal));
    }
    /// <summary>
    /// Создает вектор-столбец для расширяемой матрицы
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    private int[] GenerateVector(int ordinal)
    {
        int[] ev = new int[ordinal];
        
        for (int i = 0; i < ordinal; ++i)
            ev[i] = Random.Shared.Next(-30, 30);
        
        return ev;
    }
    /// <summary>
    /// создает общую матрицу системы
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    private int[,] GenerateMatrix(int ordinal)
    {
        int[,] matrix = new int[ordinal, ordinal];
        
        for (int i = 0; i < ordinal; ++i)
            for(int j = 0; j < ordinal; ++j)
                matrix[i,j] = Random.Shared.Next(-30, 30);

        return matrix;
    }
    
    private string? _characteristics;
    private KramerMethodModel? _model;
    /// <summary>
    /// Возвращает заглавную букву матрицы
    /// системы линейных уравнений.
    /// </summary>
    /// <returns></returns>
    public string MakeCharacteristics()
    {
        _characteristics = Convert
            .ToChar(Random.Shared.Next(0x40, 0x5A))
            .ToString();
        return _characteristics;
    }
    /// <summary>
    /// Собирает LaTeX формулу системы линейных уравнений.
    /// </summary>
    /// <returns>
    /// Строку, содержащую разметку системы
    /// линейных алгебраических уравнений
    /// </returns>
    public string GetSystemFormulaString()
    {
        return _model.MainSystemFormula;
    }

    public string GetLaTexDetFormulaString()
    {
        return _model.MainSystemSolutionFormula;
    }

    public KramerMethodModel? KramerMethodModel => 
        _model;
    
    /// <summary>
    /// Создает и решает систему линейных алгебраических уравнений
    /// </summary>
    /// <returns>
    /// Возвращает модель решения методом Крамера
    /// </returns>
    private void GenerateKramerModel(int[,] coefficients, int[] constants)
    {
        // initialize matrix word
        MakeCharacteristics();
        
        int n = constants.Length;
        KramerMethodModel model = new()
        {
            MainSystemFormula = MakeLatexSystemEquation(coefficients, constants),
            MainSystemSolutionFormula = MakeLatexDeterminant(coefficients, $"\\det{{{_characteristics}}}"),
        };
        // divide by zero
        
        for (int i = 0; i < n; i++)
        {
            int[,] modifiedMatrix = ReplaceColumn(coefficients, constants, i);
            model.MainSystemSolutionFormula += 
                MakeLatexDeterminant(modifiedMatrix, $"\\det{{{_characteristics}_{i + 1}}}");
        }

        _model = model;
    }
    
    /// <summary>
    /// Создает разметку системы линейных уравнений
    /// </summary>
    /// <param name="a">Матрица</param>
    /// <param name="b">Матрица</param>
    /// <returns></returns>
    private string MakeLatexSystemEquation(int[,] a, int[] b)
    {
        StringBuilder sb = new();
        // как указать систему линейных уравнений
        sb.AppendLine($@"{_characteristics} = \cases{{");
        
        for (int i = 0; i < b.Length; i++)
        {
            var terms = new List<string>();
            for (int j = 0; j < a.GetLength(1); j++)
            {
                terms.Add(FormatCoefficient(a[i, j], j + 1));
            }
            sb.AppendLine($"  {string.Join(" + ", terms).Replace("+ -", "- ")} = {FormatNumber(b[i])} \\\\");
        }

        sb.Append("}");
        return sb.ToString();
    }
    
    /// <summary>
    /// Создает систему уравнений с измененным вектор-стобцом
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <param name="column"></param>
    /// <returns></returns>
    private string MakeLatexSubsystemEquation(int[,] a, int[] b, int column)
    {
        StringBuilder sb = new();
        sb.Append(@"\pmatrix{");
        
        for (int i = 0; i < a.GetLength(0); i++)
        {
            var row = new List<string>();
            for (int j = 0; j < a.GetLength(1); j++)
            {
                row.Add(j == column ? FormatNumber(b[i]) : FormatNumber(a[i, j]));
            }
            sb.Append(string.Join(" & ", row) /*+ @"\\"*/);
            
        }
        
        sb.Append("}");
        return sb.ToString();
    }
    /// <summary>
    /// Создает разметку детерминанта матрицы
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    private string MakeLatexDeterminant(int[,] matrix, string label)
    {
        StringBuilder sb = new();
        sb.Append(@$"{label} = \pmatrix{{");
        
        for (int i = 0; i < matrix.GetLength(0); i++)
        {
            var row = new List<string>();
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                row.Add(FormatNumber(matrix[i, j]));
            }
            sb.Append(string.Join(" & ", row) + @"\\");
        }

        sb.Remove(sb.Length - 2, 2);
        sb.Append(@"} = ");

        double detf = Task.Run(() => Matrix.DeterminantI32Async(matrix)).Result;
        Fraction det = new(detf);

        if (Math.Floor(det.Denomerator) != 1)
            sb.Append(@$"\frac{{{det.Enumerator}}}{{{det.Denomerator}}} \\");
        else
            sb.Append(det.Enumerator + @"\\");
        
        return sb.ToString();
    }
    /// <summary>
    /// Изменяет коэффициенты главной матрицы системы
    /// </summary>
    /// <param name="value"></param>
    /// <param name="index"></param>
    /// <returns></returns>
    private string FormatCoefficient(int value, int index)
    {
        if (value == 0) return "";
        string formatted = FormatNumber(value);
        return value switch
        {
            1 => $"x_{index}",
            -1 => $"-x_{index}",
            _ => $"{formatted}x_{index}"
        };
    }
    /// <summary>
    /// Переводит число в строку
    /// </summary>
    /// <param name="number"></param>
    /// <returns></returns>
    private string FormatNumber(int number)
    {
        return number.ToString();
    }
    /// <summary>
    /// Перемещает столбец в основной матрице
    /// </summary>
    /// <param name="original"></param>
    /// <param name="column"></param>
    /// <param name="colIndex"></param>
    /// <returns></returns>
    private int[,] ReplaceColumn(int[,] original, int[] column, int colIndex)
    {
        int n = original.GetLength(0);
        int[,] newMatrix = new int[n, n];
        Array.Copy(original, newMatrix, original.Length);
        
        for (int i = 0; i < n; i++)
        {
            newMatrix[i, colIndex] = column[i];
        }
        return newMatrix;
    }
}