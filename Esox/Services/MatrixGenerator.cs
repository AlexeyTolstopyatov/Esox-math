﻿using System;
using System.Runtime.InteropServices;
using Esox.Types;

namespace Esox.Services;

public class MatrixGenerator
{
    private readonly int _ordinal;
    private readonly int _minimum;
    private readonly int _maximum;
    public MatrixGenerator(int ordinal)
    {
        _ordinal = ordinal;
        _maximum = 10;
        _minimum = -_maximum;
    }

    public MatrixGenerator(int ordinal, int minimum, int maximum)
    {
        if (maximum <= minimum)
            throw new RankException("Доигрался, гнида?");
        
        _maximum = maximum;
        _minimum = minimum;
    }
    /// <summary>
    /// Создает матрицу методом треугольных матриц.
    /// Изначально генерируется Единичная матрица
    /// порядка <see cref="_ordinal"/>
    /// и линейно преобразуется до неузноваемости.
    /// </summary>
    public (double[,], double[]) GenerateTriangleF64Matrix()
    {
        double[,] matrix = new double[_ordinal, _ordinal];
    
        // Заполнение верхней треугольной матрицы
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i; j < _ordinal; j++)
            {
                matrix[i, j] = Math.Round((double)Random.Shared.Next(_minimum, _maximum));
            }
            matrix[i, i] = (Random.Shared.Next(_minimum, _maximum) == 0) 
                ? 1 
                : -1; // гарантия ненулевого элемента
        }

        // случайные комбинации строк для "перемешивания"
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i + 1; j < _ordinal; j++)
            {
                double factor = Random.Shared.Next(_minimum, _maximum);
                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[j, k] += factor * matrix[i, k];
                }
            }
        }
            
        double[] freed = new double[_ordinal];

        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = Math.Round((double)Random.Shared.Next(_minimum, _maximum));
        }
        
        return (matrix, freed);
    }
    /// <summary>
    /// Создает матрицу методом ортогонализации
    /// (алгоритм Грама-Шмидта)
    /// преобразует последовательность линейно независимых векторов
    /// в ортонормированную систему векторов
    /// причём так, что каждый вектор есть линейная комбинация.
    /// </summary>
    public (double[,], double[]) GenerateOrthogonalF64Matrix()
    {
        double[,] matrix = new double[_ordinal, _ordinal];
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                matrix[i, j] = Math.Round((double)Random.Shared.Next(_minimum, _maximum));
            }

            for (int j = 0; j < i; j++)
            {
                double dotProduct = 0;
                for (int k = 0; k < _ordinal; k++)
                {
                    dotProduct += matrix[i, k] * matrix[j, k];
                }

                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[i, k] -= dotProduct * matrix[j, k];
                }
            }
        }

        double[] freed = new double[_ordinal];

        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = Math.Round((double)Random.Shared.Next(_minimum, _maximum));
        }
        
        return (matrix, freed);
    }
    
    public (Frac32[,], Frac32[]) GenerateOrthogonalFrac32Matrix()
    {
        Frac32[,] matrix = new Frac32[_ordinal, _ordinal];
        Frac32[] freed = new Frac32[_ordinal];
    
        // Генерация случайной матрицы с дробями
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                // Генерация дроби с случайным числителем и знаменателем 1
                matrix[i, j] = new Frac32(Random.Shared.Next(_minimum, _maximum));
            }
        }

        // Процесс ортогонализации
        for (int i = 0; i < _ordinal; i++)
        {
            // Нормализация текущего вектора
            Frac32 norm = Frac32.Sqrt(Frac32.Scalar(matrix, i, i, _ordinal));
            for (int k = 0; k < _ordinal; k++)
            {
                matrix[i, k] /= norm;
            }

            // Ортогонализация последующих векторов
            for (int j = i + 1; j < _ordinal; j++)
            {
                Frac32 projection = Frac32.Scalar(matrix, i, j, _ordinal);
            
                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[j, k] -= projection * matrix[i, k];
                }
            }
        }

        // Генерация свободных членов
        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = new Frac32(Random.Shared.Next(_minimum, _maximum));
        }
    
        return (matrix, freed);
    }
    
    public (Frac32[,], Frac32[]) GenerateTriangleFrac32Matrix()
    {
        Frac32[,] matrix = new Frac32[_ordinal, _ordinal];
    
        // Заполнение верхней треугольной матрицы
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i; j < _ordinal; j++)
            {
                // Генерация случайной дроби с знаменателем 1
                matrix[i, j] = new Frac32(Random.Shared.Next(_minimum, _maximum));
            }
        
            // Гарантия ненулевого диагонального элемента (1 или -1)
            matrix[i, i] = Random.Shared.Next(0, 2) == 0 
                ? Frac32.Positive 
                : Frac32.Negative;
        }

        // Случайные линейные комбинации строк
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i + 1; j < _ordinal; j++)
            {
                Frac32 factor = new(Random.Shared.Next(_minimum, _maximum));
            
                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[j, k] += factor * matrix[i, k];
                }
            }
        }
        
        Frac32[] freed = new Frac32[_ordinal];
        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = new Frac32(Random.Shared.Next(_minimum, _maximum));
        }
    
        return (matrix, freed);
    }
}