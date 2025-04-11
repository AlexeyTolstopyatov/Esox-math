using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace Esox.Types;

/// <summary>
/// Представляет тип обыкновенной дроби
/// Хранение данных осуществляется в <c>i32</c>
/// </summary>
public struct Frac32
{
    #region Object Interface
    private bool Equals(Frac32 other)
    {
        return Enumerator == other.Enumerator && Denominator == other.Denominator;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Frac32 other) return false;
        
        if (Enumerator == 0 && other.Enumerator == 0)
            return true;
        
        return Enumerator == other.Enumerator && Denominator == Denominator;
    }
    
    public override int GetHashCode()
    {
        return HashCode.Combine(Enumerator, Denominator);
    }
    
    #endregion
    public int Enumerator { get; private set; }
    public int Denominator { get; private set; }

    /// <summary>
    /// Создает классический "нулевой элемент"
    /// </summary>
    public static Frac32 Zero => new(0);
    /// <summary>
    /// Положительный единичный элемент
    /// </summary>
    public static Frac32 Positive = new(1);
    /// <summary>
    /// Отрицательный единичный элемент
    /// </summary>
    public static Frac32 Negative = new(-1);
    /// <summary>
    /// Переводит double значение в обыкновенную дробь
    /// </summary>
    /// <param name="f64"></param>
    public Frac32(double f64) : this(1)
    {
        (int n, int d) = GetCoefficient(f64);
        Enumerator = n;
        Denominator = d;
    }
    /// <summary>
    /// Если не указывать знаменатель,
    /// он автоматически изменяется в единицу.
    /// </summary>
    /// <param name="e">числитель</param>
    /// <param name="d">знаменатель</param>
    public Frac32(int e, int d = 1)
    {
        Enumerator = e;
        Denominator = d;
    }
    
    #region Common operations
    /// <summary>
    /// Складывает две дроби
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Frac32 operator +(Frac32 a, Frac32 b)
    {
        if (a.Denominator == b.Denominator)
            return new Frac32(a.Enumerator + b.Enumerator, a.Denominator);
        
        // 1/a + 1/b = (a + b)/ab
        int sharedD = a.Denominator * b.Denominator;
        int sharedE = (a.Enumerator * b.Denominator) + (b.Enumerator * a.Denominator);
        
        return new Frac32(sharedE, sharedD);
    }
    /// <summary>
    /// Вычитает дроби
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static Frac32 operator -(Frac32 a, Frac32 b)
    {
        int newNumerator = a.Enumerator * b.Denominator - b.Enumerator * a.Denominator;
        int newDenominator = a.Denominator * b.Denominator;
        return new Frac32(newNumerator, newDenominator);
    }
    /// <summary>
    /// Разворачивает дробь по ссылке
    /// <code>
    ///  1/2 => 1/1/2 => 2/1
    /// </code>
    /// </summary>
    /// <param name="a"></param>
    public static void Revert(ref Frac32 a)
    {
        (a.Enumerator, a.Denominator) = (a.Denominator, a.Enumerator);
    }
    /// <summary>
    /// Разворачивает дробь
    /// <code>
    /// 1/2 => 1/1/2 => 2/1
    /// </code>
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Frac32 Revert(Frac32 a)
    {
        return new Frac32(a.Denominator, a.Enumerator);
    }
    /// <summary>
    /// Разворачивает дробь
    /// <code>
    /// 1/2 => 1/1/2 => 2/1
    /// </code>
    /// </summary>
    public void Revert() => 
        (Enumerator, Denominator) = (Denominator, Enumerator);

    /// <summary>
    /// Умножает дроби
    /// </summary>
    public static Frac32 operator *(Frac32 a, Frac32 b) => 
        new(a.Enumerator * b.Enumerator, 
            a.Denominator * b.Denominator);
    
    /// <summary>
    /// Делит дроби
    /// </summary>
    public static Frac32 operator /(Frac32 a, Frac32 b) =>
        new(a.Enumerator * b.Denominator,
            a.Denominator * b.Enumerator);


    /// <summary>
    /// Сравнивает две дроби. Возвращает <c>true</c>
    /// Если дробь <see cref="a"/> больше <see cref="b"/>
    /// </summary>
    public static bool operator >(Frac32 a, Frac32 b)
    {
        // i hate myself....
        double aDoubled = a.Enumerator / a.Denominator;
        double bDoubled = b.Enumerator / b.Denominator;
        
        return aDoubled > bDoubled;
    }

    public static bool operator <(Frac32 a, Frac32 b)
    {
        return !(a > b);
    }

    /// <summary>
    /// Сравнивает дроби строго по содержанию
    /// (Если одна дробь эквивалентна другой, воозвращает false)
    /// </summary>
    /// <returns></returns>
    public static bool operator ==(Frac32 a, Frac32 b) => 
        a.Equals(b);
    
    /// <summary>
    /// Строго сравнивает дроби по содержанию
    /// (Если дроби эквивалентны, возвращает true)
    /// </summary>
    /// <param name="a"></param>
    /// <param name="b"></param>
    /// <returns></returns>
    public static bool operator !=(Frac32 a, Frac32 b) =>
        !a.Equals(b);

    /// <summary>
    /// Сокращает числитель и знаменатель
    /// дроби на наибольший общий делитель.
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Frac32 Clear(Frac32 a)
    {
        int g = GlobalDivisor(a.Denominator, a.Enumerator);
        int roundedE = a.Enumerator / g;
        int roundedD = a.Denominator / g;
        return new Frac32(roundedE, roundedD);
    }
    /// <summary>
    /// Считает модуль числителя
    /// </summary>
    /// <param name="a"></param>
    /// <returns></returns>
    public static Frac32 Abs(Frac32 a) 
        => new Frac32(Math.Abs(a.Enumerator), a.Denominator);
    /// <summary>
    /// Сокращает числитель и знаменатель дроби на наибольший
    /// общий делитель.
    /// </summary>
    public void Clear()
    {
        int g = GlobalDivisor(Denominator, Enumerator);
        Enumerator /= g;
        Denominator /= g;
    }
    /// <summary>
    /// Возвращает Скалярное произведение
    /// </summary>
    /// <returns></returns>
    public static Frac32 Scalar(Frac32[] a, Frac32[] b)
    {
        Frac32 result = Zero;
        for (int i = 0; i < a.Length; i++)
        {
            result += a[i] * b[i];
        }
        return result;
    }
    /// <summary>
    /// Возвращает скалярное произведение
    /// </summary>
    public static Frac32 Scalar(Frac32[,] matrix, int vec1, int vec2, int ordinal)
    {
        Frac32 result = Zero;
        for (int k = 0; k < ordinal; k++)
        {
            result += matrix[vec1, k] * matrix[vec2, k];
        }
        return result;
    }
    /// <summary>
    /// Берет квадратный корень из дроби
    /// </summary>
    public static Frac32 Sqrt(Frac32 f) =>
        new Frac32((int)Math.Sqrt(f.Enumerator), (int)Math.Sqrt(f.Denominator));
    
    #endregion
    /// <summary>
    /// Наибольший общий делитель для элементов
    /// дроби
    /// </summary>
    /// <returns></returns>
    private static int GlobalDivisor(int x, int y)
    {
        return y == 0 ? x : GlobalDivisor(y, x % y);
    }
    /// <summary>
    /// Переводит экземпляр обыкновенной дроби в LaTeX
    /// разметку
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        if (Enumerator == 0)
            return "0";
        
        if (Denominator == -1)
            return $"{Enumerator * -1}";
        
        return Denominator == 1
            ? $"{Enumerator}" 
            : @$"\frac{{{Enumerator}}}{{{Denominator}}}";
    }
    
    #region FracF64 class
    bool IsInteger(double x) 
    {
        return Math.Abs(Math.Floor(x) - x) < 0.001; // терпимость...
    }

    private (int n, int d) GetBinaryCoefficient(double x) 
    {
        // n / d == x
        double n = x;
        double d = 1;
        while (!IsInteger(n)) {
            // n / d == x
            n *= 2;
            d *= 2;
        }
        return (Convert.ToInt32(n), Convert.ToInt32(d));
    }

    private int[] ContinuedFraction(double n, double d) 
    {
        var f = new List<int>();
        while (d != 0) {
            f.Add((int)Math.Floor(n / d));
            (n, d) = (d, n % d);
        }
        return f.ToArray();
    }

    private (int n, int d) GetCoefficient(double x) 
    {
        int[] f = ContinuedFraction(x, 1);

        for (int i = 1; i <= f.Length; ++i) 
        {
            double n = 1;
            double d = 0;
            for (int j = i - 1; j >= 0; --j) 
            {
                (n, d) = (f[j] * n + d, n);
            }
            if (Math.Abs(n / d - x) < 0.001)
            {
                return ((int)n, (int)d);
            }
        }
        
        return GetBinaryCoefficient(x);
    }
    #endregion
}