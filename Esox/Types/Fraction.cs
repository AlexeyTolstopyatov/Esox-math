using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

namespace Esox.Services;

public class Fraction
{
    public double Enumerator { get; private set; }
    public double Denomerator { get; private set; }
    public Fraction(double floating64)
    {
        (double n, double d) = Task.Run(() => GetCoefficient(floating64)).Result;
        Enumerator = n;
        Denomerator = d;
    }
    bool IsInteger(double x) {
        return Math.Abs(Math.Floor(x) - x) < 0.001; // терпимость...
    }

    (double n, double d) GetBinaryCoefficient(double x) {
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

    double[] ContinuedFraction(double n, double d) 
    {
        var f = new List<double>();
        while (d != 0) {
            f.Add(Math.Floor(n / d));
            (n, d) = (d, n % d);
        }
        return f.ToArray();
    }

    (double n, double d) GetCoefficient(double x) 
    {
        double[] f = ContinuedFraction(x, 1);

        for (int i = 1; i <= f.Length; ++i) {
            double n = 1;
            double d = 0;
            for (int j = i - 1; j >= 0; --j) {
                (n, d) = (f[j] * n + d, n);
            }
            if (n / d == x) {
                return (n, d);
            }
        }
        return GetBinaryCoefficient(x);
    }
}