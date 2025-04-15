using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

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
    public static Frac32 DeterminantFrac32Async(Frac32[,] matrix)
    {
        int n = matrix.GetLength(0);
        
        if (n == 2)
        {
            return matrix[0, 0] * matrix[1, 1] - (matrix[0, 1] * matrix[1, 0]);
        }

        if (n == 1)
        {
            return matrix[0, 0];
        }

        List<Task<Frac32>> tasks = new List<Task<Frac32>>();
        for (int j = 0; j < n; j++)
        {
            int column = j;
            Frac32[,] minor = GetMinor(matrix, 0, column);
            tasks.Add(Task.Run(() =>
                new Frac32(Math.Pow(-1, column)) * matrix[0, column] * DeterminantFrac32Async(minor)));
        }

        Frac32[] results = Task.WhenAll(tasks).Result;
        return SumResults(results);
    }
    /// <summary>
    /// Асинхронно считает определитель матрицы
    /// для целочисленных матриц
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    public static async Task<double> DeterminantFrac32Async(int[,] matrix)
    {
        double det = await Task.Run(() => DeterminantFrac32Async(matrix));
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

    private static Frac32 SumResults(Frac32[] results)
    {
        Frac32 num1 = Frac32.Zero;
        foreach (Frac32 num2 in results)
            num1 += num2;
        
        num1.Clear();
        return num1;
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
    
    private static Frac32[,] GetMinor(Frac32[,] matrix, int row, int column)
    {
        int n = matrix.GetLength(0);
        Frac32[,] minor = new Frac32[n - 1, n - 1];
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