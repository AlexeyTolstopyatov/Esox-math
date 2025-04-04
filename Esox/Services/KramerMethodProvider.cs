﻿using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Esox.Models;
using Esox.Types;

namespace Esox.Services;

public class KramerMethodProvider : IProvider
{
    private int _ordinal;
    private List<Fraction> _determinantCollection;
    public KramerMethodProvider(int ordinal)
    {
        _determinantCollection = new List<Fraction>();
        _ = GenerateKramerModelAsync(ordinal);
    }

    private Task GenerateKramerModelAsync(int ordinal)
    {
        _ordinal = ordinal;
        GenerateKramerModel(GenerateMatrix(ordinal), GenerateVector(ordinal));
        return Task.CompletedTask;
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
    private CommonMethodComputingModel? _model;
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
    /// Содержит модель задания и решения
    /// </summary>
    public CommonMethodComputingModel Model => 
        _model!;
    
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
        
        // initialize main-system markup
        int n = constants.Length;
        CommonMethodComputingModel computingModel = new()
        {
            MainSystemFormula = MakeLatexSystemEquation(coefficients, constants),
            MainSystemSolutionFormula = MakeLatexDeterminant(coefficients, $"\\det{{{_characteristics}}}"),
        };
        
        // collect computes + initialize markup
        for (int i = 0; i < n; i++)
        {
            int[,] modifiedMatrix = ReplaceColumn(coefficients, constants, i);
            computingModel.MainSystemSolutionFormula += 
                MakeLatexDeterminant(modifiedMatrix, $"\\det{{{_characteristics}_{i + 1}}}");
        }
        
        // collect results
        computingModel.MainSystemSolutionFormula += MakeKramerResults();

        _model = computingModel;
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
    /// Создает разметку детерминанта матрицы
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="label"></param>
    /// <returns></returns>
    private string MakeLatexDeterminant(int[,] matrix, string label)
    {
        // build matrix
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
        
        // build result
        double result = Task.Run(() => Matrix.DeterminantI32Async(matrix)).Result;
        if (result == 0)
        {
            _model.MainSystemSolutionFormula = string.Empty;
            _model.MainSystemFormula = string.Empty;
            _ = GenerateKramerModelAsync(_ordinal);
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
    /// <returns></returns>
    private string MakeKramerResults()
    {
        // x1 = det[n]/det[0]
        // x2 = det[n-1]/det[0]
        // ...
        string result = $@"\\ {_characteristics} = \cases{{";
        for (int i = 1; i < _determinantCollection.Count; ++i)
            result += @$"x_{i} = \frac{{{_determinantCollection[i].Enumerator}}}{{{_determinantCollection[0].Enumerator}}}\\";
        

        result.Insert(result.Length - 2, "");
        result.Insert(result.Length - 1, "");
        result += "}";
        return result;
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
            1 => $"x_{{{index}}}",
            -1 => $"-{{x_{index}}}",
            _ => $"{{{formatted}}}x_{{{index}}}"
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