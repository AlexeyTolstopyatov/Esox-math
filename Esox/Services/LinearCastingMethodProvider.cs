using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using Esox.Models;
using Esox.Types;

namespace Esox.Services;

public enum LinearCastingGeneratorType
{
    /// <summary>
    /// Метод ортогонализации Грама-Шмидта
    /// Создает огтогональную матрицу,
    /// методом линейных случайных преобразований
    /// делает матрицу псевдо-случайных коэффициентов
    /// </summary>
    Orthogonal,
    /// <summary>
    /// Создает невырожденную
    /// матрицу из случайных элементов основываясь на
    /// генерации треугольной матрицы.
    /// Произведение элементов главной диагонали не равно нулю.
    /// Последовательности псевдослучайны.
    /// </summary>
    Triangle
}

public class LinearCastingMethodProvider : IProvider
{
    private readonly CommonMethodComputingModel? _model;
    private readonly LaTeXFrac32Markup _writer;
    private readonly MatrixGenerator _generator;
    private readonly int _ordinal;
    private readonly string _characteristics;
    private readonly bool _isHomogenous;
    private readonly bool _isSingular;
    private readonly LinearCastingGeneratorType _generatorType;
    private Frac32[,]? _extendedMatrix;
    public CommonMethodComputingModel? Model => _model;

    public LinearCastingMethodProvider(
        int ordinal, LinearCastingGeneratorType type, 
        int minimum = -10, int maximum = 10)
    {
        _generator = new MatrixGenerator(ordinal, minimum, maximum);
        _ordinal = ordinal;
        _model = new();
        _writer = new();
        _characteristics = _writer.Name;
        _generatorType = type;
        
        _isHomogenous = false;
        _isSingular = false;
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        (Frac32[,] matrix, Frac32[] vector) vectors = 
            _generatorType switch
        {
            LinearCastingGeneratorType.Orthogonal => _generator.GenerateOrthogonalFrac32Matrix(),
            LinearCastingGeneratorType.Triangle => _generator.GenerateTriangleFrac32Matrix(),
            _ => throw new InvalidCastException("Тип создания матрицы неопределен")
        };
        
        _ = Task.Run(() => WriteSystemToStringAsync(vectors));
        
        await Task.Run(() => ProcessSystem(vectors));
    }

    private Task InitializeExtendedMatrixAsync((Frac32[,] matrix, Frac32[] vector) vectors)
    {
        Frac32[,]? extendedMatrix = new Frac32[_ordinal, _ordinal + 1];
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                _extendedMatrix![i, j] = vectors.matrix[i, j];
            }
            _extendedMatrix![i, _ordinal] = vectors.vector[i];
        }

        _extendedMatrix = extendedMatrix;
        return Task.CompletedTask;
    }

    private Task WriteSystemToStringAsync((Frac32[,] matrix, Frac32[] vector) vectors)
    {
        _model!.MainSystemFormula = _writer.MakeCases(vectors.matrix, vectors.vector);
        return Task.CompletedTask;
    }

    private Task WriteSolutionToStringAsync(string text)
    {
        _model!.MainSystemSolutionFormula += text + @"\\";
        return Task.CompletedTask;
    }
    
    public (Frac32[,] matrix, Frac32[] constants) ProcessSystem((Frac32[,] matrix, Frac32[] constants) vectors)
    {
        int rows = vectors.matrix.GetLength(0);
        int cols = vectors.matrix.GetLength(1);
        Frac32[,] tempMatrix = (Frac32[,])vectors.matrix.Clone();
        Frac32[] tempConstants = (Frac32[])vectors.constants.Clone();

        for (int free = 0; free < rows; free++)
        {
            int nonZeroRow = FindFreeRow(tempMatrix, free);
            if (nonZeroRow == -1) continue;

            // Перестановка строк
            if (nonZeroRow != free)
            {
                SwapRowsByIndexes(tempMatrix, tempConstants, free, nonZeroRow);
            }

            // Нормализация строк
            Frac32 pivotElement = tempMatrix[free, free];
            if (pivotElement != Frac32.Positive)
            {
                for (int col = free; col < cols; col++)
                {
                    tempMatrix[free, col] /= pivotElement;
                }
                tempConstants[free] /= pivotElement;
            }

            // Обнуление элементов ниже ведущего
            for (int row = free + 1; row < rows; row++)
            {
                Frac32 factor = tempMatrix[row, free];
                for (int col = free; col < cols; col++)
                {
                    tempMatrix[row, col] -= factor * tempMatrix[free, col];
                }
                tempConstants[row] -= factor * tempConstants[free];
            }
        }

        return (tempMatrix, tempConstants);
    }
    /// <summary>
    /// Ищет свободную строку в матрице по индексу столбца
    /// </summary>
    private static int FindFreeRow(Frac32[,] matrix, int freeColumn)
    {
        for (int row = freeColumn; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, freeColumn] != Frac32.Zero)
                return row;
        }
        return -1;
    }
    /// <summary>
    /// Меняет строки местами
    /// </summary>
    private static void SwapRowsByIndexes(Frac32[,] matrix, Frac32[] constants, int row1, int row2)
    {
        int cols = matrix.GetLength(1);
        for (int col = 0; col < cols; col++)
        {
            (matrix[row1, col], matrix[row2, col]) = (matrix[row2, col], matrix[row1, col]);
        }
        (constants[row1], constants[row2]) = (constants[row2], constants[row1]);
    }
}
