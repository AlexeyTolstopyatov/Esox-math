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
            LinearCastingGeneratorType.Undefined => _generator.GenerateUndeterminedSystem(_ordinal -1),
            _ => _generator.GenerateRandomFrac32Matrix()
        };
        
        await WriteSystemToStringAsync(vectors);
        await WriteSolutionToStringAsync(_writer.MakePMatrix(vectors.matrix, vectors.vector));
        //await Task.Run(() => InitializeRowEchelon(vectors));
        FindSolutions(vectors.matrix, vectors.vector);
    }

    private Task<Frac32[,]> InitializeExtendedMatrixAsync((Frac32[,] matrix, Frac32[] vector) vectors)
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
    
    private Task WriteSystemToStringAsync((Frac32[,] matrix, Frac32[] vector) vectors)
    {
        _model!.MainSystemFormula = _writer.MakeCases(vectors.matrix, vectors.vector);
        return Task.CompletedTask;
    }

    private Task WriteSolutionToStringAsync(string text)
    {
        _model!.MainSystemSolutionFormula += text + @" \\ ";
        return Task.CompletedTask;
    }
    
    private (Frac32[,] matrix, Frac32[] constants) InitializeRowEchelon((Frac32[,] matrix, Frac32[] constants) vectors)
    {
        int rows = vectors.matrix.GetLength(0);
        int cols = vectors.matrix.GetLength(1);
        Frac32[,] tempMatrix = (Frac32[,])vectors.matrix.Clone();
        Frac32[] tempConstants = (Frac32[])vectors.constants.Clone();

        int currentRow = 0;
        for (int currentCol = 0; currentCol < cols && currentRow < rows; currentCol++)
        {
            // Поиск ненулевого элемента в текущем столбце
            int pivotRow = FindFreeRow(tempMatrix, currentRow, currentCol);
            
            // Все элементы в столбце нулевые
            if (pivotRow == -1) continue; 

            if (pivotRow != currentRow)
            {
                SwapRowsByIndexes(tempMatrix, tempConstants, currentRow, pivotRow);
            }

            // Нормализация ведущей строки
            Frac32 pivotElement = tempMatrix[currentRow, currentCol];
            if (pivotElement != Frac32.Positive)
            {
                for (int col = currentCol; col < cols; col++)
                {
                    tempMatrix[currentRow, col] /= pivotElement;
                    tempMatrix[currentRow, col].Clear();
                }
                tempConstants[currentRow] /= pivotElement;
                tempConstants[currentCol].Clear();
            }

            // Обнуление элементов ниже ведущего
            for (int row = currentRow + 1; row < rows; row++)
            {
                Frac32 factor = tempMatrix[row, currentCol];
                for (int col = currentCol; col < cols; col++)
                {
                    tempMatrix[row, col] -= factor * tempMatrix[currentRow, col];
                    tempMatrix[row, col].Clear();
                }
                tempConstants[row] -= factor * tempConstants[currentRow];
                tempConstants[row].Clear();
            }

            currentRow++;
        }

        WriteSolutionToStringAsync(_writer.MakeText("Ступенчатая матрица"));
        WriteSolutionToStringAsync(_writer.MakePMatrix(tempMatrix, tempConstants));
        WriteSolutionToStringAsync(_writer.MakeText("Характеристики системы"));
        WriteSolutionToStringAsync($@"r({_writer.Name}) = {Rank(tempMatrix)} \\" + 
                                   $@"n({_writer.Name}) = {_ordinal}");
        
        return (tempMatrix, tempConstants);
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
        var fsr = FindFundamentalCases(rowEchelon, rank);
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
        var fsr = FindFundamentalCases(GaussianElimination(coefficients, null).rowEchelon, rank);

        WriteSolutionToStringAsync(_writer.MakeText("Общее решение"));
        //WriteSolutionToStringAsync(_writer.MakeCases(coefficients, particular));
        
        return "rank < ordinal";
    }
    
    /// <summary>
    /// Проверяет совместность системы линейных уравнений.
    /// </summary>
    public bool IsConsistent(Frac32[,] commonMatrix, Frac32[] freeVector)
    {
        Frac32[,] extended = InitializeExtendedMatrixAsync((commonMatrix, freeVector)).Result;
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
        return (matrix, InitializeExtendedMatrixAsync((matrix, constants)).Result);
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
    
    public (Frac32[,], Frac32[]) FindSolutions(Frac32[,] matrix, Frac32[] freeTerms)
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
            extendedMatrix[i, cols] = freeTerms[i]; // Записываем свободные члены в последний столбец
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
                // Обработка случая, когда главный элемент равен нулю
                // (возможно, система несовместна или имеет бесконечно много решений)
                // В этом простом примере -> null.
                // В реализации нужно обрабатывать такие ситуации более корректно.
                return (null!, null!);
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
        WriteSolutionToStringAsync(_writer.MakeText("Извлечение решения"));
        WriteSolutionToStringAsync(_writer.MakePMatrix(extendedMatrix));
        WriteSolutionToStringAsync(_writer.MakeCases(matrix, solution));
        
        return (matrix, solution); //Возвращаем исходную матрицу, и решение.
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
