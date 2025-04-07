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
    private readonly LaTeXMarkup _writer;
    private readonly int _ordinal;
    private readonly string _characteristics;
    private readonly bool _isHomogenous;
    private readonly bool _isSingular;
    private readonly LinearCastingGeneratorType _generatorTypeType;
    private double[,] _extendedMatrix;
    public CommonMethodComputingModel? Model => _model;
    
    #region ...cctor
    public LinearCastingMethodProvider(string matrixLatex)
    {
        _ordinal = 0;
        _writer = new();
        _characteristics = _writer.Name;
        _extendedMatrix = new double[1, 1];
        _model = new CommonMethodComputingModel
        {
            MainSystemFormula = matrixLatex
        };
    }
    
    public LinearCastingMethodProvider(int ordinal, 
        bool isSingular, bool isHomogenous, 
        LinearCastingGeneratorType generatorTypeType)
    {
        _ordinal = ordinal;
        _model = new CommonMethodComputingModel();
        _writer = new LaTeXMarkup();
        _characteristics = _writer.Name;
        _isSingular = isSingular;
        _isHomogenous = isHomogenous;
        _generatorTypeType = generatorTypeType;
        _extendedMatrix = new double[_ordinal,_ordinal + 1];

        _ = GenerateModelAsync();
    }
    
    public LinearCastingMethodProvider(int ordinal, bool isHomogenous, bool isSingular, 
        LinearCastingGeneratorType generatorTypeType,
        int min, int max)
    {
        _ordinal = ordinal;
        _writer = new LaTeXMarkup();
        _model = new CommonMethodComputingModel();
        _characteristics = _writer.Name;
        _isSingular = isSingular;
        _isHomogenous = isHomogenous;
        _generatorTypeType = generatorTypeType;
        _extendedMatrix = new double[_ordinal, _ordinal + 1];
        
        _ = GenerateModelAsync(min, max);
    }
    #endregion
    #region Factory Modules
    private async Task GenerateModelAsync([Optional] int min, [Optional] int max)
    {
        _extendedMatrix = new double[_ordinal, _ordinal + 1];
        
        (double[,], double[]) tupleNd = _generatorTypeType == LinearCastingGeneratorType.Orthogonal
            ? GenerateOrthogonalMatrix(min, max) 
            : GenerateTriangleMatrix(min, max);
        
        
        WriteSystem(_writer.MakeCases2(tupleNd.Item1, tupleNd.Item2));
        WriteExtendedSystem(tupleNd.Item1, tupleNd.Item2);

        bool isUndefined = await MakeSystemCharacteristics();
    
        if (!isUndefined)
            await MakeDefinedSystemSolution();
    }
    #endregion
    #region Matrix Maker
    /// <summary>
    /// Создает не вырожденную матрицу
    /// </summary>
    /// <returns></returns>
    private (double[,], double[]) GenerateTriangleMatrix([Optional] int min, [Optional] int max)
    {
        if (min == 0 || max == 0)
        {
            min = -10;
            max = +10;
        }
        double[,] matrix = new double[_ordinal, _ordinal];
    
        // Заполнение верхней треугольной матрицы
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i; j < _ordinal; j++)
            {
                matrix[i, j] = Random.Shared.Next(min, max);
            }
            matrix[i, i] = (Random.Shared.Next(min, max) == 0) 
                ? 1 
                : -1; // гарантия ненулевого элемента
        }

        // Добавляем случайные комбинации строк для "перемешивания"
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i + 1; j < _ordinal; j++)
            {
                double factor = Random.Shared.Next(min, max);
                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[j, k] += factor * matrix[i, k];
                }
            }
        }
            
        double[] freed = new double[_ordinal];

        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = Random.Shared.Next(min, max);
        }
        
        return (matrix, freed);
    }
    /// <summary>
    /// Задает случайными числами матрицы
    /// </summary>
    /// <returns></returns>
    private (double[,], double[]) GenerateOrthogonalMatrix([Optional] int min, [Optional] int max)
    {
        if (min == 0 || max == 0)
        {
            min = -10;
            max = 10;
        }
        double[,] matrix = new double[_ordinal, _ordinal];
        for (int i = 0; i < _ordinal; i++)
        {
            // случайный вектор
            for (int j = 0; j < _ordinal; j++)
            {
                matrix[i, j] = Random.Shared.Next(min, max);
                if (Math.Abs(matrix[i, j] - int.MaxValue) < 0 ||
                    Math.Abs(matrix[i, j] - int.MinValue) < 0)
                    matrix[i, j] = Random.Shared.Next(min, max);
            }

            // Ортогонализация относительно предыдущих строк
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
            freed[i] = Random.Shared.Next(min, max);
        }
        
        return (matrix, freed);
    }
    #endregion
    #region Output modules

    private void WriteSolution(string content)
    {
        _model!.MainSystemSolutionFormula += content;
    }

    private void WriteSystem(string content)
    {
        _model!.MainSystemFormula += content;
    }
    #endregion
    #region Rank
    /// <summary>
    /// Считает ранг матрицы
    /// </summary>
    /// <param name="matrix"></param>
    /// <returns></returns>
    private int Rank(double[,] matrix) 
    {
        int rang = 0;
        int q = 1;
 
        while (q <= Minimum(matrix.GetLength(0), matrix.GetLength(1))) 
        {
            double[,] matbv = new double[q, q];
            for (int i = 0; i < (matrix.GetLength(0) - (q - 1)); i++)
            {
                for (int j = 0; j < (matrix.GetLength(1) - (q - 1)); j++) 
                {
                    for (int k = 0; k < q; k++) 
                    {
                        for (int c = 0; c < q; c++)
                        {
                            matbv[k, c] = matrix[i + k, j + c];
                        }
                    }
                    if (Matrix.DeterminantF64(matbv) != 0) 
                    {
                        rang = q; 
                    }
                }
            }
            q++;
        }
        return rang;
    }
    /// <summary>
    /// Считает минимум из двух значений
    /// </summary>
    /// <returns>
    /// </returns>
    private int Minimum(int a, int b)
    {
        return a < b ? a : b;
    }
    #endregion

    #region Asynchronous
    /// <summary>
    /// Я переименую этот метод позже.
    /// Характеристики Системы линейных уравнений
    /// в данном случае подразумевают порядок, ранг матрицы
    /// и вердикт согласно теореме Кранекера-Капелли.
    /// </summary>
    /// <returns>
    /// Возвращает истину, если система имеет множество решений.
    /// </returns>
    private Task<bool> MakeSystemCharacteristics()
    {
        int rank = Rank(_extendedMatrix);
        WriteSolution(_writer.MakeText("Характеристики системы"));
        WriteSolution(@$"n({_writer.Name}) = {_ordinal} \\");
        WriteSolution(@$"r({_writer.Name}) = {rank} \\");
        WriteSolution(_writer.MakeText("Возможность множества решений " + (_ordinal != rank)));
        
        return Task.FromResult(_ordinal != rank);
    }
    /// <summary>
    /// Разрешает определенную систему линейных уравнений
    /// </summary>
    private async Task MakeDefinedSystemSolution()
    {
        await Task.Run(() =>
        {
            // 1. Прямой ход (приведение к ступенчатому виду)
            for (int i = 0; i < _extendedMatrix.GetLength(0); i++)
            {
                // Поиск максимального элемента в столбце (частичный выбор главного элемента)
                int maxRow = i;
                for (int k = i + 1; k < _extendedMatrix.GetLength(0); k++)
                {
                    if (Math.Abs(_extendedMatrix[k, i]) > Math.Abs(_extendedMatrix[maxRow, i]))
                    {
                        maxRow = k;
                    }
                }

                // Перестановка строк, если необходимо
                if (maxRow != i)
                {
                    SwapRowsByIndex(i, maxRow);
                    WriteSolution(_writer.MakePMatrixWithText("Перестановка строк " + (i + 1) + " и " + (maxRow + 1), false, _extendedMatrix));
                }

                // Нормализация текущей строки
                double pivot = _extendedMatrix[i, i];
                if (pivot == 0)
                {
                    WriteSolution(_writer.MakeText("Матрица вырождена. Решение не может быть найдено"));
                    return; // Выходим из асинхронной задачи
                }
                for (int j = i; j < _extendedMatrix.GetLength(1); j++)
                {
                    _extendedMatrix[i, j] = Math.Round(pivot / _extendedMatrix[i, j]);
                }
                WriteSolution(_writer.MakePMatrixWithText("Нормализация строки " + (i + 1), false, _extendedMatrix));
                
                // Обнуление элементов под диагональю
                for (int k = i + 1; k < _extendedMatrix.GetLength(0); k++)
                {
                    double factor = _extendedMatrix[k, i];
                    for (int j = i; j < _extendedMatrix.GetLength(1); j++)
                    {
                        _extendedMatrix[k, j] -= factor * _extendedMatrix[i, j];
                    }
                    WriteSolution(_writer.MakePMatrixWithText("Вычитание строки " + (i + 1) + " умноженной на " + Math.Round(factor) + " из строки " + (k + 1), false, _extendedMatrix));
                }
            }

            // 2. Обратный ход (приведение к единичной матрице)
            for (int i = _extendedMatrix.GetLength(0) - 1; i >= 0; i--)
            {
                for (int k = i - 1; k >= 0; k--)
                {
                    double factor = _extendedMatrix[k, i];
                    for (int j = i; j < _extendedMatrix.GetLength(1); j++)
                    {
                        _extendedMatrix[k, j] -= factor * _extendedMatrix[i, j];
                    }
                    WriteSolution(_writer.MakePMatrixWithText(
                        "Вычитание строки " + (i + 1) + " умноженной на " + Math.Round(factor) + " из строки " + (k + 1) + " (обратный ход)",
                        false, _extendedMatrix));
                }
            }
            // 3. Перезапись в виде системы линейных уравнений
            WriteSolution(_writer.MakeText("Решение"));
            ReadExtendedSystem(_extendedMatrix, out double[,] common, out double[] freed);
            WriteSolution(_writer.MakeCases2(common, freed));
        });
    }
    /// <summary>
    /// Разрешает неопределенную систему линейных уравнений
    /// </summary>
    private Task MakeUndefinedSystemSolution()
    {
        return Task.CompletedTask;
    }
    #endregion
    #region Fundamental Solutions
    /// <summary>
    /// Фундаментальная совокупность решений
    /// однородной СЛАУ
    /// </summary>
    /// <param name="matrixRowEchelon"></param>
    /// <returns>
    /// Зубчатый массив
    /// </returns>
    private double[,] FindFundamentalSolutions(double[,] matrixRowEchelon)
    {
        int rank = Rank(matrixRowEchelon);
        int paramsCount = matrixRowEchelon.GetLength(1);
        int freedCount = paramsCount - rank;

        List<int> basicVars = new();
        List<int> freeVars = new();

        for (int row = 0, col = 0; row < rank && col < paramsCount; col++)
        {
            if (Math.Abs(matrixRowEchelon[row, col]) > 1e-10) 
            {
                basicVars.Add(col);
                row++;
            }
            else
            {
                freeVars.Add(col);
            }
        }
        
        double[,] fundamentals = new double[paramsCount, freedCount];

        for (int i = 0; i < freedCount; i++)
        {
            int currentFreeVar = freeVars[i];
            for (int varIndex = 0; varIndex < paramsCount; varIndex++)
            {
                // Свободная переменная
                if (freeVars.Contains(varIndex)) 
                {
                    // Для i-й свободной переменной ставим 1, остальным 0
                    fundamentals[varIndex, i] = (varIndex == currentFreeVar) ? 1 : 0;
                }
                else
                {
                    //fundamentals[varIndex, i] = FindBasicParameters(matrixRowEchelon, varIndex, currentFreeVar);
                }
            }
        }
        return fundamentals;
    }
    #endregion
    #region Savers
    /// <summary>
    /// Собирает расширенную матрицу
    /// системы линейных алгебраических уравнений в <c>_extendedSystem</c>
    /// </summary>
    /// <param name="common">Основная Матрица</param>
    /// <param name="freed">Столбец свободных коэффициентов</param>
    /// <returns></returns>
    private void WriteExtendedSystem(double[,] common, double[] freed)
    {
        _extendedMatrix = new double[_ordinal, _ordinal + 1];
        
        // Инициализация расширенной матрицы
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                _extendedMatrix[i, j] = common[i, j];
            }
            _extendedMatrix[i, _ordinal] = freed[i];
        }
        WriteSolution(_writer.MakePMatrixWithText($"Запишем расширенную матрицу системы", false, _extendedMatrix));
    }
    /// <summary>
    /// Разделяет расширенную матрицу на основную матрицу и вектор свободных членов.
    /// </summary>
    /// <param name="extended">Расширенная матрица.</param>
    /// <param name="common">Основная матрица (выходной параметр).</param>
    /// <param name="freed">Вектор свободных членов (выходной параметр).</param>
    /// <returns>True, если разделение прошло успешно (матрица не пустая и имеет правильную структуру), иначе False.</returns>
    private static void ReadExtendedSystem(double[,] extended, out double[,] common, out double[] freed)
    {
        common = null!;
        freed = null!;

        if (extended == null!)
        {
            return;
        }

        int rows = extended.GetLength(0);
        int cols = extended.GetLength(1);

        if (rows == 0 || cols < 2)
        {
            return;
        }

        common = new double[rows, cols - 1];
        freed = new double[rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                common[i, j] = extended[i, j];
            }
            freed[i] = extended[i, cols - 1];
        }
    }
    #endregion
    /// <summary>
    /// Изменяет порядок строк
    /// </summary>
    /// <param name="row1">Столбец который изменяется</param>
    /// <param name="row2">Столбец которым заменяется</param>
    private void SwapRowsByIndex(int row1, int row2)
    {
        for (int j = 0; j < (_ordinal + 1); j++)
        {
            (_extendedMatrix[row1, j], _extendedMatrix[row2, j]) = 
                (_extendedMatrix[row2, j], _extendedMatrix[row1, j]);
        }
    }
}
