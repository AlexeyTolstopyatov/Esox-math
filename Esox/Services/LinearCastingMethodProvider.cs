using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
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
    private readonly int _rank;
    private readonly string _characteristics;
    private readonly bool _isHomogenous;
    private readonly bool _isUndefined;
    private readonly bool _isConsistent;
    public CommonMethodComputingModel? Model => _model;

    public LinearCastingMethodProvider(
        int ordinal, int rank, LinearCastingGeneratorType type,
        bool isConsistent,
        bool isUndefined,
        int minimum = -10, int maximum = 10)
    {
        _generator = new MatrixGenerator(ordinal, minimum, maximum);
        _ordinal = ordinal;
        _rank = rank;
        _model = new();
        _writer = new();
        _characteristics = _writer.Name;
        _generatorType = type;
        _isConsistent = isConsistent;
        _isHomogenous = false;
        _isUndefined = isUndefined;
        
        _ = InitializeAsync();
    }
    
    private async Task InitializeAsync()
    {
        (Frac32[,] matrix, Frac32[] vector) vectors;
        if (!_isConsistent)
        {
            vectors = _generator.GenerateInConsistentFrac32Matrix(_rank);
        }
        else if (_isUndefined)
        {
            vectors = _generator.GenerateUndefinedFrac32Matrix(_rank);
        }
        else
        {
            vectors = _generatorType switch
            {
                LinearCastingGeneratorType.Orthogonal => _generator.GenerateOrthogonalFrac32MatrixC(),
                LinearCastingGeneratorType.Triangle => _generator.GenerateTriangleFrac32Matrix(),
                _ => _generator.GenerateRandomFrac32Matrix()
            };
        }
        
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
    
    /// <summary>
    /// Находит индексы главных и временных переменных
    /// в нормальной фундаментальной совокупности решений
    /// </summary>
    /// <param name="extendedMatrix">матрица коэффициентов</param>
    public static (int[], int[]) FindBasisAndFreeVariables(ref Frac32[,] extendedMatrix)
    {
        int rows = extendedMatrix.GetLength(0);
        int cols = extendedMatrix.GetLength(1);
        List<int> basisVars = new List<int>();
        List<int> freeVars = new List<int>();

        for (int j = 0; j < cols - 1; j++)
        {
            bool isBasis = false;
            for (int i = 0; i < rows; i++)
            {
                if (extendedMatrix[i, j].Denominator == 1 &&
                    extendedMatrix[i, j].Enumerator == 1)
                {
                    basisVars.Add(j);
                    isBasis = true;
                    break;
                }
            }
            if (!isBasis)
            {
                freeVars.Add(j);
            }
        }
        return (basisVars.ToArray(), freeVars.ToArray());
    }

    /// <summary>
    /// Ищет фундаментальную совокупность решений
    /// для системы линейных алгебраических уравнений
    /// </summary>
    /// <exception cref="AggregateException">Система не совместна</exception>
    private void FindFundamentalCases(ref Frac32[,] extendedMatrix)
    {
        (int[] baseParameters, int[] freeParameters) parameters = 
            FindBasisAndFreeVariables(ref extendedMatrix);
        FromExtendedMatrix(ref extendedMatrix, out Frac32[,] matrix, out Frac32[] freed);
        
        int freeParametersCount = parameters.freeParameters.Length;
        int baseParametersCount = parameters.baseParameters.Length;

        List<Frac32[]> fsr = new();
        Frac32[] particularSolution = new Frac32[matrix.GetLength(1) - 1]; // Частное решение

        // 1. частное решение (все свободные переменные = 0)
        for (int i = 0; i < matrix.GetLength(1) - 1; i++)
        {
            if (parameters.baseParameters.Contains(i))
            {
                int rowIndex = -1;
                for (int r = 0; r < matrix.GetLength(0); r++)
                {
                    if (matrix[r, i].Enumerator != 1 || matrix[r, i].Denominator != 1) 
                        continue;
                    
                    rowIndex = r;
                    break;
                }
                if (rowIndex != -1)
                {
                    particularSolution[i] = freed[rowIndex];
                }
                else
                {
                    particularSolution[i] = Frac32.Zero; // Базисная переменная равна нулю
                }
            }
            else
            {
                particularSolution[i] = Frac32.Zero; // Свободная переменная
            }
        }

        // 2. решения ФСР
        for (int i = 0; i < freeParametersCount; i++)
        {
            Frac32[] solution = new Frac32[matrix.GetLength(1)];
            for (int j = 0; j < matrix.GetLength(1); j++)
            {
                solution[j] = Frac32.Zero;
            }

            int freeVarIndex = parameters.freeParameters[i];
            solution[freeVarIndex] = new Frac32(1);

            for (int k = 0; k < baseParametersCount; k++)
            {
                int basisVarIndex = parameters.baseParameters[k];
                int rowIndex = -1;
                for (int r = 0; r < matrix.GetLength(0); r++)
                {
                    if (matrix[r, basisVarIndex].Enumerator == 1 && matrix[r, basisVarIndex].Denominator == 1)
                    {
                        rowIndex = r;
                        break;
                    }
                }
                if (rowIndex != -1)
                {
                    foreach (var freeParameter in parameters.freeParameters)
                    {
                        solution[basisVarIndex] -= matrix[rowIndex, freeParameter] * Frac32.Positive;
                    }
                }
                else
                {
                    solution[basisVarIndex] = Frac32.Zero;
                }
            }
            fsr.Add(solution);
        }
        
        // Собрана Фундаментальная совокупность решений...
        // solution = [[\frac, \frac, 1, 0], [\frac, \frac, 0, 1]]
        Frac32[,] fundament = new Frac32[baseParametersCount, _ordinal];

        for (int i = 0; i < baseParametersCount; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                fundament[i, j] = fsr
                    .ElementAt(i)
                    .ElementAt(j);
            }
        }

        WriteSolutionToStringAsync(_writer.MakeText($"Фундаментальная совокупность решений системы"));
        WriteSolutionToStringAsync(_writer.MakePMatrix(fundament, $"{_writer.Name}_{{fnd}}"));
        
    }

    public static void PrintParticularSolution(Frac32[] particularSolution)
    {
        if (particularSolution != null && particularSolution.Length > 0)
        {
            Console.WriteLine("Particular Solution:");
            Console.Write("[");
            foreach (var val in particularSolution)
            {
                Console.Write($"{val} ");
            }

            Console.Write("] ");
            Console.WriteLine();
        }
    }

    /// <summary>
    /// Рассчитывает и возвращает фундаментальную совокупность решений
    /// </summary>
    private (int[] baseIndexes, int[] tempIndexes) FindFundamentalCases(Frac32[,] matrix, int rank)
    {
        // Сделать однородную систему
        FromExtendedMatrix(ref matrix, out Frac32[,] coefficients, out _);
        Frac32[] zeroConstants = new Frac32[_ordinal];
        MatrixGenerator.Initialize(ref zeroConstants);

        Frac32[,] homoMatrix = ToExtendedMatrixAsync((coefficients, zeroConstants)).Result;
        
        int vars = _ordinal;
        int freeVars = vars - rank;
        List<int> baseIndexesList = new();
        List<int> tempIndexesList = new();
        
        for (int i = 0; i < freeVars; i++)
        {
            var vector = new Frac32[vars];
            
            int allParametersCount = homoMatrix.GetLength(1); // Общее количество переменных
            
            // Находим индексы базисных и свободных переменных
            // x_1 = t_3 [....] + t_4 [....]
            // x_2 = t_3 [....] + t_4 [....]

            for (int row = 0, col = 0; row < rank && col < allParametersCount; col++)
            {
                if (homoMatrix[row, col].Enumerator != 0) // Нашли ведущий элемент
                {
                    baseIndexesList.Add(col);
                    row++;
                }
                else
                {
                    tempIndexesList.Add(col);
                }
            }
        }
        // ok, [x_n, x_m] ok [t_s, t_p]
        string baseIndexesString = baseIndexesList.Aggregate("", (current, t) => current + @$"x_{t + 1}; ");
        string tempIndexesString = tempIndexesList.Aggregate("", (current, t) => current + $"t_{t + 1};");
        WriteSolutionToStringAsync(_writer.MakeText("Основные переменные ") + baseIndexesString);
        WriteSolutionToStringAsync(_writer.MakeText("Временные переменные") + tempIndexesString);
        
        return (baseIndexesList.ToArray(), tempIndexesList.ToArray());
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
        int cols = matrix.GetLength(1);
        if (rows != freeTerms.Length)
        {
            throw new ArgumentException("Количество строк матрицы не совпадает с длиной вектора свободных членов.");
        }
        
        // 1. Расширенная матрица системы
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
                // Проверить основную и расширенную матрицу
                // Если SingleSolutionExists(ref extendedMatrix)...
                // Продолжить поиск решений.
                // Иначе искать н-ФСР
                if (HasSolutionSet(ref extendedMatrix)) 
                    return;
                
                FindFundamentalCases(ref extendedMatrix);
                
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

    /// <summary>
    /// Проверяет расширенную систему линейных уравнений.
    /// Существует ли множество решений системы или система
    /// имеет лишь одно.
    /// </summary>
    /// <param name="extended">Расширенная матрица системы</param>
    /// <returns>
    /// <c>True</c>
    /// Если система имеет множество решений,
    /// в любом другом случае <c>False</c>
    /// </returns>
    private bool HasSolutionSet(ref Frac32[,] extended)
    {
        WriteSolutionToStringAsync(_writer.MakeText("Характеристики системы"));
        FromExtendedMatrix(ref extended, out Frac32[,] coefficients, out Frac32[] _);
        int r_ex = Rank(extended);
        int c_ex = Rank(coefficients);
        
        WriteSolutionToStringAsync(@$"n({_writer.Name}) = {_ordinal} \\");
        WriteSolutionToStringAsync(@$"r({_writer.Name}_{{ex}}) = {r_ex} \\");
        WriteSolutionToStringAsync($@"r({_writer.Name}) = {c_ex} \\");
        if (r_ex != c_ex)
        {
            WriteSolutionToStringAsync(_writer.MakeText($@"Система уравнений несовместна"));
            return false;
        }

        if (r_ex == c_ex && c_ex < _ordinal)
        {
            WriteSolutionToStringAsync(_writer.MakeText("Система уравнений неопределена"));
            return true;
        }
        
        return false;
    }
}
