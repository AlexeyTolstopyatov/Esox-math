using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esox.Types;

/// <summary>
/// Внимание, числитель и знаменатель
/// дроби сокращаются при вычислении обыкновенной дроби
/// </summary>
public class Fraction
{
    /// <summary>
    /// Числитель
    /// </summary>
    public int Enumerator { get; private set; }
    /// <summary>
    /// Знаменатель
    /// </summary>
    public int Denomerator { get; private set; }
    
    public Fraction(double floating64)
    {
        (int n, int d) = Task.Run(() => GetCoefficient(floating64)).Result;
        Enumerator = n;
        Denomerator = d;
    }

    private static bool IsInteger(double x) 
    {
        return Math.Abs(Math.Floor(x) - x) < 0.001; // терпимость...
    }

    /// <summary>
    /// Создает обыкновенную дробь кратную двум
    /// (сокращает результат на наибольший общий делитель)
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private static (int n, int d) GetDoubledCoefficient(double x) 
    {
        // n / d == x
        double n = x;
        int d = 1;
        while (!IsInteger(n)) 
        {
            // n / d == x
            n *= 2;
            d *= 2;
        }
        return ((int)Math.Round(n), d);
    }

    /// <summary>
    /// Переводит обыкновенную дробь в десятичную бесконечную
    /// </summary>
    /// <param name="n"></param>
    /// <param name="d"></param>
    /// <returns></returns>
    private int[] ContinuedFraction(double n, double d) 
    {
        var f = new List<int>();
        while (d != 0) 
        {
            f.Add((int)Math.Floor(n / d));
            (n, d) = (d, n % d);
        }
        return f.ToArray();
    }

    /// <summary>
    /// Вычисляет обыкновенную дробь на основе
    /// десятичной. Сокращает на наибольший общий делитель
    /// </summary>
    /// <param name="x"></param>
    /// <returns></returns>
    private (int n, int d) GetCoefficient(double x) 
    {
        int[] f = ContinuedFraction(x, 1);

        for (int i = 1; i <= f.Length; ++i) 
        {
            int n = 1;
            int d = 0;
            for (int j = i - 1; j >= 0; --j) 
            {
                (n, d) = (f[j] * n + d, n);
            }
            if (n / d == x)
            {
                int div = GlobalDivisor(n, d);
                return (n / div, d / div);
            }
        }
        return GetDoubledCoefficient(x);
    }
    
    /// <summary>
    /// Наибольший общий делитель для элементов
    /// дроби
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static int GlobalDivisor(int x, int y)
    {
        return y == 0 ? x : GlobalDivisor(y, x % y);
    }
}