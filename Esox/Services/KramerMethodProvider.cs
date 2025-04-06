using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Esox.Models;
using Esox.Types;

namespace Esox.Services;

public class KramerMethodProvider : IProvider
{
    private readonly int _ordinal;
    private readonly List<Fraction> _determinantCollection;
    private readonly LaTeXMarkup _writer;

    // public KramerMethodProvider(double[,] common, double[] freed)
    // {
    //     _determinantCollection = new();
    //     _writer = new();
    //     _model = new(); 
    //     // solve it.
    //     throw new NotImplementedException("Нет возможности решить матрицу системы");
    // }
    
    public KramerMethodProvider(int ordinal, int min, int max)
    {
        _ordinal = ordinal;
        _determinantCollection = new();
        _writer = new();
        _model = new();
        _ = GenerateKramerModelAsync(min, max);
    }
    public KramerMethodProvider(int ordinal)
    {
        _ordinal = ordinal;
        _determinantCollection = new List<Fraction>();
        _writer = new();
        _model = new();
        _ = GenerateKramerModelAsync();
    }
    /// <summary>
    /// Создает и решает систему линейных уравнений методом Крамера
    /// </summary>
    /// <returns></returns>
    private async Task GenerateKramerModelAsync()
    {
        await Task.Run(() =>
        {
            GenerateKramerModel(
                GenerateMatrix(), 
                GenerateVector());
        });
    }

    private async Task GenerateKramerModelAsync(int min, int max)
    {
        await Task.Run(() =>
        {
            GenerateKramerModel(
                GenerateMatrix(min, max), 
                GenerateVector(min, max));
        });
    }

    /// <summary>
    /// Создает вектор-столбец для расширяемой матрицы.
    /// Значения минимума и максимума влияют на
    /// диапозон псевдо-случайных чисел.
    /// </summary>
    /// <param name="min">минимальное значение</param>
    /// <param name="max">максимальное значение</param>
    /// <returns></returns>
    private double[] GenerateVector([Optional] int min, [Optional] int max)
    {
        double[] ev = new double[_ordinal];
        
        for (int i = 0; i < _ordinal; ++i)
            ev[i] = Random.Shared.Next(
                min == 0 ? -30 : min, 
                max == 0 ? 30 : max);
        
        return ev;
    }

    /// <summary>
    /// создает общую матрицу системы. Значения минимума и максимума влияют на
    /// диапозон псевдо-случайных чисел
    /// </summary>
    /// <param name="min">минимальное значение</param>
    /// <param name="max">максимальное значение</param>
    /// <returns></returns>
    private double[,] GenerateMatrix([Optional] int min, [Optional] int max)
    {
        double[,] matrix = new double[_ordinal, _ordinal];
        
        for (int i = 0; i < _ordinal; ++i)
            for(int j = 0; j < _ordinal; ++j)
                matrix[i,j] = Random.Shared.Next(
                    min == 0 ? -30 : min,
                    max == 0 ? +30 : max);

        return matrix;
    }
    
    private string? _characteristics;
    private readonly CommonMethodComputingModel? _model;
    /// <summary>
    /// Содержит модель задания и решения
    /// </summary>
    public CommonMethodComputingModel? Model => 
        _model;
    
    /// <summary>
    /// Создает и решает систему линейных алгебраических уравнений
    /// </summary>
    /// <returns>
    /// Возвращает модель решения методом Крамера
    /// </returns>
    private void GenerateKramerModel(double[,] coefficients, double[] constants)
    {
        // initialize matrix word
        _characteristics = _writer.Name;
        
        // initialize main-system markup
        WriteSystemString(_writer.MakeCases(coefficients, constants));
        WriteSolutionString(_writer.MakePMatrix(coefficients, constants));
        WriteSolutionString(MakeLatexDeterminant(coefficients, $"\\det{{{_characteristics}}}"));
        
        // collect computes + initialize markup
        for (int i = 0; i < constants.Length; i++)
        {
            double[,] modifiedMatrix = ReplaceColumn(coefficients, constants, i);
            WriteSolutionString(MakeLatexDeterminant(modifiedMatrix, $"\\det{{{_characteristics}_{{{i + 1}}}}}"));
        }
        
        // collect results
        WriteSolutionString(MakeKramerResults());
    }

    private void WriteSolutionString(string markup)
    {
        _model!.MainSystemSolutionFormula += markup + @"\\";
    }

    private void WriteSystemString(string markup)
    {
        _model!.MainSystemFormula += markup + @"\\";
    }
    
    /// <summary>
    /// Создает разметку детерминанта матрицы
    /// </summary>
    /// <param name="matrix">Определитель основной матрицы</param>
    /// <param name="label">Наименование</param>
    /// <returns></returns>
    private string MakeLatexDeterminant(double[,] matrix, string label)
    {
        StringBuilder sb = new();
        sb.Append(label + " = ");
        
        // build result
        double result = Task.Run(() => Matrix.DeterminantF64Async(matrix)).Result;
        
        // singular matrix situation
        if (result == 0)
        {
            _model!.MainSystemSolutionFormula = string.Empty;
            _model!.MainSystemFormula = string.Empty;
            _ = GenerateKramerModelAsync();
        }
        
        Fraction detF = new(result);
        
        if (detF.Denomerator != 1)
            sb.Append(@$"\frac{{{detF.Enumerator}}}{{{detF.Denomerator}}}");
        else
            sb.Append(detF.Enumerator);

        sb.Append(@"\\");
        SetDeterminant(detF);
        
        return sb.ToString();
    }
    /// <summary>
    /// Запоминает определитель матрицы
    /// </summary>
    /// <param name="f"></param>
    private void SetDeterminant(Fraction f)
    {
        _determinantCollection.Add(f);
    }
    /// <summary>
    /// Собирает результаты (отношения определителей)
    /// </summary>
    private string MakeKramerResults()
    {
        // x1 = det[n]/det[0]
        // x2 = det[n-1]/det[0]
        // ...
        string result = $@"\\ {_characteristics} = \cases{{";
        for (int i = 1; i < _determinantCollection.Count; ++i)
            result += @$"x_{i} = \frac{{{_determinantCollection[i].Enumerator}}}{{{_determinantCollection[0].Enumerator}}}\\";
        
        _ = result.Insert(result.Length - 2, "");
        _ = result.Insert(result.Length - 1, "");
        result += "}";
        return result;
    }
    /// <summary>
    /// Перемещает столбец в основной матрице
    /// </summary>
    /// <param name="original"></param>
    /// <param name="column"></param>
    /// <param name="colIndex"></param>
    /// <returns></returns>
    private static double[,] ReplaceColumn(double[,] original, double[] column, int colIndex)
    {
        int n = original.GetLength(0);
        double[,] newMatrix = new double[n, n];
        Array.Copy(original, newMatrix, original.Length);
        
        for (int i = 0; i < n; i++)
        {
            newMatrix[i, colIndex] = column[i];
        }
        return newMatrix;
    }
}