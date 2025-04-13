using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Mime;
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
    Orthogonal = 0,
    /// <summary>
    /// Создает невырожденную
    /// матрицу из случайных элементов основываясь на
    /// генерации треугольной матрицы.
    /// Произведение элементов главной диагонали не равно нулю.
    /// Последовательности псевдослучайны.
    /// </summary>
    Triangle = 1,
    /// <summary>
    /// Создает матрицу из псевдослучайных элементов
    /// Никто не знает, какой у нее определитель будет,
    /// никто не знает будет ли она совместна или несовместна
    /// </summary>
    Random = 2,
    /// <summary>
    /// Создает неопределенную совместную систему
    /// линейных алгебраических уравнений
    /// </summary>
    Undefined = 3,
}

public class LinearCastingMethodProvider : IProvider
{
    private readonly CommonMethodComputingModel? _model;
    private readonly LaTeXFrac32Markup _writer;
    private readonly MatrixGenerator _generator;
    private readonly LinearCastingGeneratorType _generatorType;
    private readonly int _ordinal;
    private readonly string _characteristics;
    private readonly bool _isHomogenous;
    private readonly bool _isSingular;
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
            LinearCastingGeneratorType.Undefined => _generator.GenerateRandomFrac32Matrix(),
            _ => _generator.GenerateUndeterminedSystem(_ordinal - 1)
        };
        
        await WriteSystemToStringAsync(vectors);
        await WriteSolutionToStringAsync(_writer.MakePMatrix(vectors.matrix, vectors.vector));
        
        FindSolutions(vectors.matrix, vectors.vector);
    }
    /// <summary>
    /// Заполняет расширенную матрицу системы
    /// из кортежа основных векторов
    /// </summary>
    /// <param name="vectors">кортеж векторов системы</param>
    private Task<Frac32[,]> ToExtendedMatrixAsync((Frac32[,] matrix, Frac32[] vector) vectors)
    {
        Frac32[,] extendedMatrix = new Frac32[_ordinal, _ordinal + 1];
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                extendedMatrix[i, j] = vectors.matrix[i, j];
            }
            extendedMatrix[i, _ordinal] = vectors.vector[i];
        }

        //_extendedMatrix = extendedMatrix;
        return Task.FromResult<Frac32[,]>(extendedMatrix);
    }
    /// <summary>
    /// Разделяет расширенную матрицу на основную матрицу и вектор свободных членов.
    /// </summary>
    /// <param name="extended">Расширенная матрица.</param>
    /// <param name="common">Основная матрица (выходной параметр).</param>
    /// <param name="freed">Вектор свободных членов (выходной параметр).</param>
    /// <returns>True, если разделение прошло успешно (матрица не пустая и имеет правильную структуру), иначе False.</returns>
    private static void FromExtendedMatrix(ref Frac32[,] extended, out Frac32[,] common, out Frac32[] freed)
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

        common = new Frac32[rows, cols - 1];
        freed = new Frac32[rows];

        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols - 1; j++)
            {
                common[i, j] = extended[i, j];
            }
            freed[i] = extended[i, cols - 1];
        }
    }
    /// <summary>
    /// Записывает данные в поле системы линейных уравнений
    /// </summary>
    /// <param name="vectors">Кортеж векторов матрицы</param>
    private Task WriteSystemToStringAsync((Frac32[,] matrix, Frac32[] vector) vectors)
    {
        _model!.MainSystemFormula = _writer.MakeCases(vectors.matrix, vectors.vector);
        return Task.CompletedTask;
    }
    /// <summary>
    /// Дописывает данные в поле решения систем
    /// линейных уравнений
    /// </summary>
    /// <param name="text">Текст рещения в LaTeX</param>
    private Task WriteSolutionToStringAsync(string text)
    {
        _model!.MainSystemSolutionFormula += text + @" \\ ";
        return Task.CompletedTask;
    }
    /// <summary>
    /// Ищет свободную строку в матрице по индексу столбца
    /// </summary>
    private static int FindFreeRow(Frac32[,] matrix, int startRow, int col)
    {
        for (int row = startRow; row < matrix.GetLength(0); row++)
        {
            if (matrix[row, col] != Frac32.Zero)
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
    
    private string Solve(Frac32[,] coefficients, Frac32[] constants)
    {
        if (!_isHomogenous && constants == null)
            throw new ArgumentException("Неоднородная система требует констант.");

        // Приводим матрицу к ступенчатому виду
        var (rowEchelon, extendedMatrix) = GaussianElimination(coefficients, constants);
        int rank = Rank(rowEchelon);
        int emRank = Rank(extendedMatrix);

        // Генерация LaTeX с шагами преобразований
        WriteSolutionToStringAsync(_writer.MakeText("Преобразование матрицы"));
        WriteSolutionToStringAsync(_writer.MakePMatrix(coefficients, constants));

        if (!_isHomogenous)
        {
            if (rank != emRank)
                return "Система несовместна. Ранги не совпадают.";

            // WriteSolutionToStringAsync(_writer.MakeText("Система не однородна"));
            // WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));
        }

        // Обработка решений
        return _isHomogenous 
            ? ProcessHomogeneous(rank, rowEchelon) 
            : ProcessNonHomogeneous(rank, coefficients, constants, rowEchelon, extendedMatrix);
    }

    /// <summary>
    /// Решает однородную систему линейных уравнений
    /// </summary>
    /// <param name="rank">Ранг матрицы</param>
    /// <param name="rowEchelon">Ступенчатая матрица</param>
    /// <returns></returns>
    private string ProcessHomogeneous(int rank, Frac32[,] rowEchelon)
    {
        if (rank == _ordinal)
            return _writer.MakeText("Тривиальное решение: все переменные равны нулю.");

        // Нахождение ФСР
        List<Frac32[]> fsr = FindFundamentalCases(rowEchelon, rank);
        WriteSolutionToStringAsync(_writer.MakeText("Фундаментальная совокупность решений"));
        //WriteSolutionToStringAsync(_writer.MakePMatrix(rowEchelon));
        return "";
    }

    private string ProcessNonHomogeneous(int rank, Frac32[,] coefficients, Frac32[] constants,
        Frac32[,] rowEchelon, Frac32[,] extendedMatrix)
    {
        if (rank == _ordinal)
        {
            //var solution = BackSubstitution(rowEchelon);
            // return _writer.MakePMatrix(/*solution*/);
            return "Факир был пьян.";
        }

        // Общее решение: частное + ФСР
        //var particular = FindParticularSolution(rowEchelon);
        var fsr = FindFundamentalCases(GaussianElimination(coefficients, null!).rowEchelon, rank);

        WriteSolutionToStringAsync(_writer.MakeText("Общее решение"));
        //WriteSolutionToStringAsync(_writer.MakeCases(coefficients, particular));
        
        return "rank < ordinal";
    }
    
    /// <summary>
    /// Проверяет совместность системы линейных уравнений.
    /// </summary>
    public bool IsConsistent(Frac32[,] commonMatrix, Frac32[] freeVector)
    {
        Frac32[,] extended = ToExtendedMatrixAsync((commonMatrix, freeVector)).Result;
        return Rank(commonMatrix) == Rank(extended);
    }

    /// <summary>
    /// Считает ранг матрицы
    /// </summary>
    private int Rank(Frac32[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int rank = 0;
        
        for (int row = 0; row < rows; row++)
        {
            // Проверяем, является ли строка ненулевой
            bool isZeroRow = true;
            for (int col = 0; col < cols; col++)
            {
                // |matrix[row, column]| > epsilon
                if (matrix[row, col] != Frac32.Zero)
                {
                    isZeroRow = false;
                    break;
                }
            }
            if (!isZeroRow) rank++;
        }

        return rank;
    }
    
    private (Frac32[,] rowEchelon, Frac32[,] augmentedMatrix) GaussianElimination(Frac32[,] matrix, Frac32[] constants)
    {
        // Реализация преобразования матрицы...
        return (matrix, ToExtendedMatrixAsync((matrix, constants)).Result);
    }
    
    /// <summary>
    /// Рассчитывает и возвращает фундаментальную совокупность решений
    /// </summary>
    private List<Frac32[]> FindFundamentalCases(Frac32[,] matrix, int rank)
    {
        int vars = _ordinal;
        int freeVars = vars - rank;
        var fsr = new List<Frac32[]>();

        for (int i = 0; i < freeVars; i++)
        {
            var vector = new Frac32[vars];
            // Логика выбора свободных переменных
            // и выражения базисных через них
            
            int allParametersCount = matrix.GetLength(1); // Общее количество переменных
            int freeParametersCount = allParametersCount - rank; // Количество свободных переменных

            // Находим индексы базисных и свободных переменных
            List<int> basicParameters = new List<int>();
            List<int> freeParameters = new List<int>();

            for (int row = 0, col = 0; row < rank && col < allParametersCount; col++)
            {
                if (matrix[row, col] != Frac32.Zero) // Нашли ведущий элемент
                {
                    basicParameters.Add(col);
                    row++;
                }
                else
                {
                    freeParameters.Add(col);
                }
            }
            fsr.Add(vector);
        }
        return fsr;
    }
    
    /// <summary>
    /// Ищет решения для системы линейных уравнений
    /// </summary>
    /// <exception cref="ArgumentException">
    /// Количество строк матрицы не совпадает с длиной вектора свободных членов.
    /// </exception>
    private void FindSolutions(Frac32[,] matrix, Frac32[] freeTerms)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1); // количество переменных
        if (rows != freeTerms.Length)
        {
            throw new ArgumentException("Количество строк матрицы не совпадает с длиной вектора свободных членов.");
        }

        // 1. Создаем расширенную матрицу
        Frac32[,] extendedMatrix = new Frac32[rows, cols + 1];
        for (int i = 0; i < rows; i++)
        {
            for (int j = 0; j < cols; j++)
            {
                extendedMatrix[i, j] = matrix[i, j];
            }
            extendedMatrix[i, cols] = freeTerms[i];
        }

        // 2. Прямой ход
        for (int i = 0; i < rows; i++)
        {
            // 2.1 Поиск главного элемента
            int freeRow = i;
            for (int k = i + 1; k < rows; k++)
            {
                if (Math.Abs((double)extendedMatrix[k, i].Enumerator / extendedMatrix[k, i].Denominator) > 
                    Math.Abs((double)extendedMatrix[freeRow, i].Enumerator / extendedMatrix[freeRow, i].Denominator))
                {
                    freeRow = k;
                }
            }
            // 2.2 Перестановка строк (если необходимо)
            if (freeRow != i)
            {
                SwapRows(extendedMatrix, i, freeRow);
            }
            WriteSolutionToStringAsync(_writer.MakeText("Перестановка строк"));
            WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));
            
            // 2.3 Нормализация (делаем главный элемент равным 1)
            Frac32 free = extendedMatrix[i, i];
            if (free.Enumerator == 0)
            {
                return;
            }
            
            for (int j = i; j <= cols; j++)
            {
                extendedMatrix[i, j] /= free;
                extendedMatrix[i, j].Clear();
            }
            WriteSolutionToStringAsync(_writer.MakeText("Нормализация строк"));
            WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));
            
            // 2.4 Обнуление элементов под главным элементом
            for (int k = i + 1; k < rows; k++)
            {
                Frac32 factor = extendedMatrix[k, i];
                for (int j = i; j <= cols; j++)
                {
                    extendedMatrix[k, j] -= factor * extendedMatrix[i, j];
                    extendedMatrix[k,j].Clear();
                }
            }
            WriteSolutionToStringAsync(_writer.MakeText("Зануление элементов"));
            WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));
        }
        WriteSolutionToStringAsync(_writer.MakeText("Характеристики системы"));
        WriteSolutionToStringAsync(@$"n({_writer.Name}) = {_ordinal} \\");
        WriteSolutionToStringAsync(@$"r({_writer.Name}) = {Rank(extendedMatrix)} \\");

        // Проверить основную и расширенную матрицу
        // Если SingleSolutionExists(ref extendedMatrix)...
        // Продолжить поиск решений.
        // Иначе искать н-ФСР
        
        // 3. Обратный ход
        for (int i = rows - 1; i >= 0; i--)
        {
            for (int k = i - 1; k >= 0; k--)
            {
                Frac32 factor = extendedMatrix[k, i];
                for (int j = i; j <= cols; j++)
                {
                    extendedMatrix[k, j] -= factor * extendedMatrix[i, j];
                    extendedMatrix[k, j].Clear();
                }
            }
        }
        WriteSolutionToStringAsync(_writer.MakeText("Обратный ход"));
        WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));

        // 4. Извлечение решения
        Frac32[] solution = new Frac32[cols];
        for (int i = 0; i < rows; i++)
        {
            solution[i] = extendedMatrix[i, cols];
            solution[i].Clear();
        }
        
        FromExtendedMatrix(ref extendedMatrix, out Frac32[,] coefficients, out Frac32[] constants);
        
        WriteSolutionToStringAsync(_writer.MakeText("Извлечение решения"));
        WriteSolutionToStringAsync(_writer.MakeCases(coefficients, constants));
    }
    
    private void SwapRows(Frac32[,] matrix, int row1, int row2)
    {
        int cols = matrix.GetLength(1);
        for (int j = 0; j < cols; j++)
        {
            (matrix[row1, j], matrix[row2, j]) = (matrix[row2, j], matrix[row1, j]);
        }
    }
}
