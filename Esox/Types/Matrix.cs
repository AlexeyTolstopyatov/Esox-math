using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Esox.Types;

/// <summary>
/// Представляет возможности рассчета операций для
/// элементов матрицы или матриц
/// </summary>
public static class Matrix
{
    /// <summary>
    /// Считает определитель матрицы
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static double DeterminantF64(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        switch (n)
        {
            case 1: return matrix[0, 0];
            case 2: return matrix[0, 0] * matrix[1, 1] - matrix[0, 1] * matrix[1, 0];
        }

        double determinant = 0;
        for (int j = 0; j < n; j++)
        {
            double[,] minor = GetMinor(matrix, 0, j);
            determinant += matrix[0, j] * Math.Pow(-1, j) * DeterminantF64(minor);
        }
        return determinant;
    }
    /// <summary>
    /// Синхронно счититает определитель матрицы
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static double DeterminantI32(int[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
    
        double[,] result = new double[rows, cols];
    
        Parallel.For(0, rows, i =>
        {
            for (int j = 0; j < cols; j++)
            {
                result[i, j] = matrix[i, j];
            }
        });
        
        return DeterminantF64(result);
    }
    /// <summary>
    /// Асинхронно считает определитель матрицы
    /// для целочисленных матриц
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static async Task<double> DeterminantI32Async(int[,] matrix)
    {
        double det = await Task.Run(() => DeterminantI32(matrix));
        return det;
    }
    /// <summary>
    /// Асинхронно считает определитель
    /// для матриц с элементами <see cref="double"/>
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static async Task<double> DeterminantF64Async(double[,] matrix)
    {
        int n = matrix.GetLength(0);
        if (n <= 2) return DeterminantF64(matrix);

        List<Task<double>> tasks = new List<Task<double>>();
        for (int j = 0; j < n; j++)
        {
            int column = j;
            double[,] minor = GetMinor(matrix, 0, column);
            tasks.Add(Task.Run(() => 
                Math.Pow(-1, column) * matrix[0, column] * DeterminantF64(minor)));
        }

        double[] results = await Task.WhenAll(tasks);
        return SumResults(results);
    }

    private static double SumResults(double[] results)
    {
        return results.Sum();
    }

    private static double[,] GetMinor(double[,] matrix, int row, int column)
    {
        int n = matrix.GetLength(0);
        double[,] minor = new double[n - 1, n - 1];
        for (int i = 0, ii = 0; i < n; i++)
        {
            if (i == row) continue;
            for (int j = 0, jj = 0; j < n; j++)
            {
                if (j == column) continue;
                minor[ii, jj] = matrix[i, j];
                jj++;
            }
            ii++;
        }
        return minor;
    }
}