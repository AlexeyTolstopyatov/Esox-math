using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Esox.Types;

namespace Esox.Services;

public class MatrixGenerator
{
    private readonly int _ordinal;
    private readonly int _minimum;
    private readonly int _maximum;
    private readonly bool _homogenous;
    public MatrixGenerator(int ordinal, [Optional] bool homo)
    {
        _ordinal = ordinal;
        _maximum = 10;
        _minimum = -_maximum;
        _homogenous = homo;
    }
    
    public MatrixGenerator(int ordinal, int minimum, int maximum, [Optional] bool homo)
    {
        if (maximum <= minimum)
            throw new RankException("Доигрался, гнида?");
        _ordinal = ordinal;
        _maximum = maximum;
        _minimum = minimum;
        _homogenous = homo;
    }

    /// <summary>
    /// Инициализирует матрицу коэффициентов.
    /// Заполняет нулями.
    /// </summary>
    /// <param name="matrix"></param>
    public static void Initialize(ref Frac32[,] matrix)
    {
        for (int i = 0; i < matrix.GetLength(0); i++)
        for (int j = 0; j < matrix.GetLength(1); j++)
            matrix[i, j] = Frac32.Zero;
    }

    /// <summary>
    /// Заполняет вектор-столбец нулями
    /// </summary>
    /// <param name="vector"></param>
    public static void Initialize(ref Frac32[] vector)
    {
        for(int i = 0; i < vector.Length; ++i)
            vector[i] = Frac32.Zero;
    }
    
    /// <summary>
    /// Создает матрицу коэффициентов и постоянных
    /// методом Ортогонализации Грама-Шмидта
    /// </summary>
    /// <returns>Матрицу коэффициентов и вектор свободных членов</returns>
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
                matrix[i, j] = new Frac32(
                    Random.Shared.Next(_minimum, _maximum),
                    Random.Shared.Next(1, _maximum));
                //matrix[i, j].Clear();
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
                //matrix[i, k].Clear();
            }

            // Ортогонализация последующих векторов
            for (int j = i + 1; j < _ordinal; j++)
            {
                Frac32 projection = Frac32.Scalar(matrix, i, j, _ordinal);
            
                for (int k = 0; k < _ordinal; k++)
                {
                    matrix[j, k] -= projection * matrix[i, k];
                    //matrix[j, k].Clear();
                }
            }
        }
        if (_homogenous)
        {
            Initialize(ref freed);
            return (matrix, freed);
        }
        // Генерация свободных членов
        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = new Frac32(Random.Shared.Next(_minimum, _maximum));
            //freed[i].Clear();
        }
    
        return (matrix, freed);
    }

    /// <summary>
    /// Создает матрицу псевдо-случайными коэффициентами
    /// </summary>
    /// <returns>
    /// Вектор-столбец свободных членов
    /// и матрицу коэффициентов
    /// </returns>
    public (Frac32[,], Frac32[]) GenerateRandomFrac32Matrix()
    {
        Frac32[,] matrix = new Frac32[_ordinal, _ordinal];
        Frac32[] constants = new Frac32[_ordinal];
        
        for (int i = 0; i < _ordinal; ++i)
        for(int j = 0; j < _ordinal; ++j)
            matrix[i,j] = new(Random.Shared.Next(_minimum, _maximum));

        if (_homogenous)
        {
            Initialize(ref constants);
            return (matrix, constants);
        }
        
        for (int i = 0; i < _ordinal; ++i)
            constants[i] = new(Random.Shared.Next(_minimum, _maximum));
        
        return (matrix, constants);
    }
    
    /// <summary>
    /// Создает линейно-преобразованную
    /// треугольную матрицу коэффициентов и вектор-столбец
    /// свободных членов
    /// </summary>
    /// <remarks>
    /// Определитель единичной треугольной матрицы
    /// всегда равен 1 или -1
    /// </remarks>
    /// <returns>
    /// Вектор-столбец свободных членов
    /// и матрицу коэффициентов
    /// </returns>
    public (Frac32[,], Frac32[]) GenerateTriangleFrac32Matrix()
    {
        Frac32[,] matrix = new Frac32[_ordinal, _ordinal];
        Initialize(ref matrix);
        
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
                : new Frac32(-1);
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
        Initialize(ref freed);
        
        if (_homogenous)
            return (matrix, freed);
        
        for (int i = 0; i < _ordinal; ++i)
        {
            freed[i] = new Frac32(Random.Shared.Next(_minimum, _maximum));
        }
    
        return (matrix, freed);
    }
    /// <summary>
    /// Создает 
    /// </summary>
    /// <param name="rank"></param>
    /// <returns></returns>
    /// <exception cref="ArgumentException"></exception>
    public (Frac32[,], Frac32[]) GenerateUndeterminedSystem(int rank)
    {
        if (rank >= _ordinal) 
            throw new ArgumentException("Ранг должен быть меньше числа переменных");

        Frac32[,] coefficients = new Frac32[_ordinal, _ordinal];
        Initialize(ref coefficients);
        
        for (int i = 0; i < rank; i++)
        {
            coefficients[i, i] = Frac32.Positive; // Единичная матрица для базиса
        }

        for (int i = rank; i < _ordinal; i++)
        {
            for (int j = 0; j < _ordinal; j++)
            {
                coefficients[i, j] = coefficients[i - rank, j];
            }
        }
        // Случайные линейные комбинации строк
        for (int i = 0; i < _ordinal; i++)
        {
            for (int j = i + 1; j < _ordinal; j++)
            {
                Frac32 factor = new(Random.Shared.Next(_minimum, _maximum));
            
                for (int k = 0; k < _ordinal; k++)
                {
                    coefficients[j, k] += factor * coefficients[i, k];
                }
            }
        }
        Frac32[] constants = new Frac32[_ordinal];
        Initialize(ref constants);

        if (_homogenous)
            return (coefficients, constants);
        
        
        for (int i = 0; i < _ordinal; i++)
        {
            constants[i] = (i < rank) 
                ? new(Random.Shared.Next(_minimum, _maximum)) 
                : constants[i - rank];
        }

        return (coefficients, constants);
    }
}