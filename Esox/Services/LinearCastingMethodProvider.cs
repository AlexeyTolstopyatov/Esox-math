using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Esox.Models;
using Esox.Types;

namespace Esox.Services;

public enum LinearCastMaker
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
    private readonly string _characteristics;
    private readonly LaTeXMarkup _writer;
    private readonly int _ordinal;
    private readonly bool _isHomogenous;
    private readonly bool _isSingular;
    private readonly LinearCastMaker _makerType;
    
    public LinearCastingMethodProvider(string matrixLatex)
    {
        _ordinal = 0;
        _writer = new LaTeXMarkup();
        _model = new CommonMethodComputingModel
        {
            MainSystemFormula = matrixLatex
        };
    }
    
    public LinearCastingMethodProvider(int ordinal, bool isSingular, bool isHomogenous, LinearCastMaker makerType)
    {
        _ordinal = ordinal;
        _writer = new LaTeXMarkup();
        _model = new();
        _characteristics = _writer.Name;
        
        // Теорема Кранекера-Капелли.
        // Если ранг матрицы ниже чем порядок матрицы --
        // система имеет сегмент решений
        
        _isHomogenous = isHomogenous;
        _isSingular = isSingular;
        _ = GenerateModel();
    }

    public LinearCastingMethodProvider(int ordinal, bool isHomogenous, bool isSingular, LinearCastMaker makerType,
        int min, int max)
    {
        _ordinal = ordinal;
        _writer = new LaTeXMarkup();
        _model = new();
        _characteristics = _writer.Name;
        
        // Теорема Кранекера-Капелли.
        // Если ранг матрицы ниже чем порядок матрицы --
        // система имеет сегмент решений
        
        _isHomogenous = isHomogenous;
        _isSingular = isSingular;
        _ = GenerateModel(min, max);
    }

    #region Asyncronous parts

    private async Task GenerateModel([Optional] int min, [Optional] int max)
    {
        await Task.Run(() =>
        {
            if (_isSingular)
            {
                (double[,], double[]) tuple = 
                    GenerateSingularSystem(_ordinal);
                MakeSingularSolutionLatex(tuple.Item1, tuple.Item2);
                return;
            }
        
            _extendedMatrix = new double[_ordinal, _ordinal + 1];
            _solution = new();

            (double[,], double[]) tupleNd = (_makerType == LinearCastMaker.Orthogonal) 
                ? MakeOrthogonalMatrix(_ordinal) 
                : MakeNonSingularMatrix(_ordinal, new Random());
        
            MakeExtendedSystem(tupleNd.Item1, tupleNd.Item2);
            MakeExtendedSystemSolutions(out string s, _ordinal);
        
            _model!.MainSystemSolutionFormula = s;
            _model!.MainSystemFormula = "";
        });
    }

    #endregion
    
    
    public CommonMethodComputingModel? Model => _model;
    
    #region Singular System
    /// <summary>
    /// Создает вырожденную систему линейных уравнений.
    /// (определитель матрицы равен нулю)
    /// </summary>
    /// <returns></returns>
    private (double[,] matrix, double[] constants) GenerateSingularSystem([Optional] int min, [Optional] int max)
    {
        double[,] matrix = new double[_ordinal, _ordinal];
        double[] constants = new double[_ordinal];

        for (int i = 0; i < _ordinal - 1; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                matrix[i, j] = Random.Shared.Next(-30, 30);
            }
            constants[i] = Random.Shared.Next(-30, 30);
        }

        for (int j = 0; j < _ordinal; j++)
        {
            matrix[_ordinal - 1, j] = matrix[0, j] + matrix[1, j];
        }
        constants[_ordinal - 1] = constants[0] + constants[1];
        
        return (matrix, constants);
    }
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

    private int Minimum(int a, int b)
    {
        return a < b ? a : b;
    }
    
    /// <summary>
    /// Создает разметку решения системы линейных уравнений
    /// </summary>
    /// <param name="matrix"></param>
    /// <param name="constants"></param>
    /// <returns></returns>
    private void MakeSingularSolutionLatex(double[,] matrix, double[] constants)
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
            sb.Append(@$" = {constants[i]} \\");
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
        sb.Append(@"} = 0 \\");
        // Ранг
        sb.Append($@"r({_characteristics}) = {Rank(matrix)}");
        
        // Общее решение
        sb.Append(@"\\ \cases{");
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
    #endregion

    #region Common System
    private double[,] _extendedMatrix;
    private StringBuilder _solution;
    
    /// <summary>
    /// Создает не вырожденную матрицу
    /// </summary>
    /// <param name="n"></param>
    /// <param name="rand"></param>
    /// <returns></returns>
    private static (double[,], double[]) MakeNonSingularMatrix(int n, Random rand)
    {
        double[,] matrix = new double[n, n];
    
        // Заполнение верхней треугольной матрицы
        for (int i = 0; i < n; i++)
        {
            for (int j = i; j < n; j++)
            {
                matrix[i, j] = rand.Next(-10, 10);
            }
            matrix[i, i] = (rand.Next(-10, 10) == 0) 
                ? 1 
                : -1; // гарантия ненулевого элемента
        }

        // Добавляем случайные комбинации строк для "перемешивания"
        for (int i = 0; i < n; i++)
        {
            for (int j = i + 1; j < n; j++)
            {
                double factor = rand.Next(-10, 10);
                for (int k = 0; k < n; k++)
                {
                    matrix[j, k] += factor * matrix[i, k];
                }
            }
        }
            
        double[] freed = new double[n];

        for (int i = 0; i < n; ++i)
        {
            freed[i] = Random.Shared.Next(-10, 10);
        }
        
        return (matrix, freed);
    }
    
    /// <summary>
    /// Задает случайными числами матрицы
    /// </summary>
    /// <param name="n"></param>
    /// <returns></returns>
    private static (double[,], double[]) MakeOrthogonalMatrix(int n)
    {
        double[,] matrix = new double[n, n];
        for (int i = 0; i < n; i++)
        {
            // случайный вектор
            for (int j = 0; j < n; j++)
            {
                matrix[i, j] = Random.Shared.Next(-10, 10);
            }

            // Ортогонализация относительно предыдущих строк
            for (int j = 0; j < i; j++)
            {
                double dotProduct = 0;
                for (int k = 0; k < n; k++)
                {
                    dotProduct += matrix[i, k] * matrix[j, k];
                }

                for (int k = 0; k < n; k++)
                {
                    matrix[i, k] -= dotProduct * matrix[j, k];
                }
            }
        }

        double[] freed = new double[n];

        for (int i = 0; i < n; ++i)
        {
            freed[i] = Random.Shared.Next(-10, 10);
        }
        
        return (matrix, freed);
    }
    /// <summary>
    /// Собирает расширенную матрицу
    /// системы линейных алгебраических уравнений
    /// </summary>
    /// <param name="expectedExtended"></param>
    /// <param name="freeCoefficients"></param>
    /// <returns></returns>
    private void MakeExtendedSystem(double[,] expectedExtended, double[] freeCoefficients)
    {
        int n = expectedExtended.GetLength(0);
        _extendedMatrix = new double[n, n + 1];
        _solution = new StringBuilder();
        
        // Инициализация расширенной матрицы
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
            {
                _extendedMatrix[i, j] = expectedExtended[i, j];
            }
            _extendedMatrix[i, n] = freeCoefficients[i];
        }
        MakeLatexFormulaStep($"{_characteristics} = ", n);
    }
    /// <summary>
    /// Разрешает расширенную систему уравнений
    /// методом Гаусса. (или методом линейных преобразований)
    /// </summary>
    /// <param name="latexReport"></param>
    /// <param name="limits"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
    private double[] MakeExtendedSystemSolutions(out string latexReport, int limits)
    {
        try
        {
            GaussianElimination(limits);
            return BackSubstitution(out latexReport, limits);
        }
        catch (InvalidOperationException ex)
        {
            latexReport = _solution.ToString();
        }

        return new double[]{};
    }

    /// <summary>
    /// Исключение по Гауссу это сокращение строк,
    /// представляет собой алгоритм решения систем линейных уравнений.
    /// </summary>
    /// <param name="limits"></param>
    /// <exception cref="InvalidOperationException"></exception>
    private void GaussianElimination(int limits)
    {
        for (int i = 0; i < limits; i++)
        {
            // Выбор ведущего элемента
            int freed = FindFreeRow(i, limits);
            if (Math.Abs(_extendedMatrix[freed, i]) < 1e-3)
                throw new InvalidOperationException("Матрица вырождена");

            if (freed != i)
            {
                SwapRows(i, freed, limits);
                MakeLatexFormulaStep($"Перестановка строк {i + 1} и {freed + 1}:", limits);
            }

            // Нормализация строки
            NormalizeRow(i, limits);
            MakeLatexFormulaStep($"Нормализация строки {i + 1}:", limits);

            // Исключение
            for (int j = i + 1; j < limits; j++)
            {
                EliminateRow(j, i, limits);
                MakeLatexFormulaStep($"Исключение в строке {j + 1} используя строку {i + 1}:", limits);
            }
        }
    }

    /// <summary>
    /// Реализация обратной подстановки. Возвращает главную матрицу системы
    /// в разрешенном виде.
    /// </summary>
    /// <param name="latex"></param>
    /// <param name="limits"></param>
    /// <returns></returns>
    private double[] BackSubstitution(out string latex, int limits)
    {
        double[] solution = new double[limits];
        StringBuilder solutionBuilder = new();

        for (int i = limits - 1; i >= 0; i--)
        {
            solution[i] = _extendedMatrix[i, limits];
            for (int j = i + 1; j < limits; j++)
            {
                solution[i] -= _extendedMatrix[i, j] * solution[j];
            }
        }

        solutionBuilder.Append(@"\text{Решение} \\\\");
        solutionBuilder.Append(@"\cases{");
        for (int i = 0; i < limits; i++)
        {
            solutionBuilder.Append($"x_{{{i + 1}}} = {solution[i]} \\\\");
        }
        solutionBuilder.Append("}");
        
        latex = _solution.ToString() + solutionBuilder;
        return solution;
    }

    /// <summary>
    /// Ищет свободный стобец
    /// </summary>
    /// <param name="col"></param>
    /// <param name="n"></param>
    /// <returns></returns>
    private int FindFreeRow(int col, int n)
    {
        int maxRow = col;
        for (int i = col + 1; i < n; i++)
        {
            if (Math.Abs(_extendedMatrix[i, col]) > 
                Math.Abs(_extendedMatrix[maxRow, col]))
            {
                maxRow = i;
            }
        }
        return maxRow;
    }
    /// <summary>
    /// Изменяет порядок строк
    /// </summary>
    /// <param name="row1"></param>
    /// <param name="row2"></param>
    /// <param name="limits"></param>
    private void SwapRows(int row1, int row2, int limits)
    {
        for (int j = 0; j <= limits; j++)
        {
            (_extendedMatrix[row1, j], _extendedMatrix[row2, j]) = (_extendedMatrix[row2, j], _extendedMatrix[row1, j]);
        }
    }
    /// <summary>
    /// Нормализует строку
    /// </summary>
    /// <param name="row"></param>
    /// <param name="limits"></param>
    private void NormalizeRow(int row, int limits)
    {
        double divisor = _extendedMatrix[row, row];
        for (int j = row; j <= limits; j++)
        {
            _extendedMatrix[row, j] /= divisor;
        }
    }

    /// <summary>
    /// Удаляет ненужную строку
    /// (согласно методу линейных преобразований)
    /// </summary>
    /// <param name="targetRow"></param>
    /// <param name="freedRow"></param>
    /// <param name="limits"></param>
    private void EliminateRow(int targetRow, int freedRow, int limits)
    {
        double factor = _extendedMatrix[targetRow, freedRow];
        for (int j = freedRow; j <= limits; j++)
        {
            _extendedMatrix[targetRow, j] -= factor * _extendedMatrix[freedRow, j];
        }
    }
    /// <summary>
    /// Добавляет шаг под указанным названием в
    /// формулу
    /// </summary>
    /// <param name="description"></param>
    /// <param name="n"></param>
    private void MakeLatexFormulaStep(string description, int n)
    {
        _solution.Append($@"\text{{{description}}} \\");
        _solution.Append(@"\pmatrix{");
        
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j <= n; j++)
            {
                // if Fraction -> new Fraction()
                Fraction f = new(_extendedMatrix[i, j]);
                _solution.Append(f.Denomerator == 1
                    ? $"{(int)f.Enumerator}" 
                    : @$"\frac{{ {(int)f.Enumerator} }}{{ {(int)f.Denomerator} }}");
                
                if (j < n) _solution.Append(" & ");
            }
            _solution.Append(@" \\");
        }
        _solution.Insert(_solution.Length - 2, "");
        _solution.Insert(_solution.Length - 1, "");
        _solution.AppendLine("}");
    }
    
    #endregion
}
