using System;
using System.Text;
using Esox.Models;

namespace Esox.Services;

public class LinearCastingMethodProvider : IProvider
{
    private CommonMethodComputingModel _model;
    private string _characteristics; 
    
    public LinearCastingMethodProvider(int ordinal, bool degenerate, bool homogenous)
    {
        _model = new();
        _characteristics = MakeCharacteristics();

        if (degenerate)
        {
            (double[,], double[]) tuple = MakeDegenerateSystem(ordinal);
            MakeDegenerateSolutionLatex(tuple.Item1, tuple.Item2);
        }
    }

    public CommonMethodComputingModel Model => _model;

    /// <summary>
    /// Возвращает заглавную букву матрицы
    /// системы линейных уравнений.
    /// </summary>
    /// <returns></returns>
    public string MakeCharacteristics()
    {
        return Convert
            .ToChar(Random.Shared.Next(0x40, 0x5A))
            .ToString();;
    }
    
    /// <summary>
    /// Создает вырожденную систему линейных уравнений.
    /// (определитель матрицы равен нулю)
    /// </summary>
    /// <param name="ordinal"></param>
    /// <returns></returns>
    private (double[,] matrix, double[] constants) MakeDegenerateSystem(int ordinal)
    {
        double[,] matrix = new double[ordinal, ordinal];
        double[] constants = new double[ordinal];

        for (int i = 0; i < ordinal - 1; i++)
        {
            for (int j = 0; j < ordinal; j++)
            {
                matrix[i, j] = Random.Shared.Next(-30, 30);
            }
            constants[i] = Random.Shared.Next(-30, 30);
        }

        for (int j = 0; j < ordinal; j++)
        {
            matrix[ordinal - 1, j] = matrix[0, j] + matrix[1, j];
        }
        constants[ordinal - 1] = constants[0] + constants[1];

        return (matrix, constants);
    }
    /// <summary>
    /// Создает разметку решения системы линейных уравнений
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="constants"></param>
    /// <returns></returns>
    private void MakeDegenerateSolutionLatex(double[,] matrix, double[] constants)
    {
        int n = constants.Length;
        StringBuilder sb = new();

        // Запись системы
        sb.Append($"{_characteristics} = ");
        sb.Append(@"\cases{");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                sb.Append(j == 0 
                    ? $"{matrix[i, j]}x_{j + 1}" 
                    : $"+ {matrix[i, j]}x_{j + 1}");
            }
            sb.Append($" = {constants[i]} \\\\");
        }
        sb.Append(@"}");
        
        // Запись решения.
        _model.MainSystemFormula = sb.ToString();
        sb.Clear();
        
        sb.Append(@$"\\ \det{{{_characteristics}}} = \pmatrix{{");
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                sb.Append($"{matrix[i, j]} & ");
            }
            sb.Remove(sb.Length - 2, 2).Append(@"\\ ");
        }
        sb.Append("} = 0");

        // Общее решение
        sb.Append(@"\\ \text{Общее решение} \\");
        sb.Append(@"\cases{");
        sb.Append(@"x_1 = " + (constants[0] - 2) + @" - t \\");
        for (int i = 1; i < n; i++)
        {
            sb.Append($@"x_{i + 1} = t_{i} \\");
        }

        sb.Insert(sb.Length - 2, "");
        sb.Insert(sb.Length - 1, "");
        sb.Append(@"}");

        _model.MainSystemSolutionFormula = sb.ToString();
    }
}