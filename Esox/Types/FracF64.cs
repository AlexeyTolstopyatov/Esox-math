﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Esox.Types;

public class FracF64
{
    /// <summary>
    /// Числитель
    /// </summary>
    public double Enumerator { get; private set; }
    /// <summary>
    /// Знаменатель
    /// </summary>
    public double Denomerator { get; private set; }
    public FracF64(double floating64)
    {
        (double n, double d) = Task.Run(() => GetCoefficient(floating64)).Result;
        Enumerator = n;
        Denomerator = d;
    }
    bool IsInteger(double x) 
    {
        return Math.Abs(Math.Floor(x) - x) < 0.001; // терпимость...
    }

    private (double n, double d) GetBinaryCoefficient(double x) 
    {
        // n / d == x
        double n = x;
        double d = 1;
        while (!IsInteger(n)) {
            // n / d == x
            n *= 2;
            d *= 2;
        }
        return (n, d);
    }

    private double[] ContinuedFraction(double n, double d) 
    {
        var f = new List<double>();
        while (d != 0) {
            f.Add(Math.Floor(n / d));
            (n, d) = (d, n % d);
        }
        return f.ToArray();
    }

    private (double n, double d) GetCoefficient(double x) 
    {
        double[] f = ContinuedFraction(x, 1);

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
                double g = GlobalDivisor(n, d);
                return (n / g, d / g);
            }
        }
        
        return GetBinaryCoefficient(x);
    }
    /// <summary>
    /// Наибольший общий делитель для элементов
    /// дроби
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <returns></returns>
    private static double GlobalDivisor(double x, double y)
    {
        return y == 0 ? x : GlobalDivisor(y, x % y);
    }
}