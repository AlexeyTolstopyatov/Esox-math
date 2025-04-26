using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esox.Types;

namespace Esox.Services;

public static class LaTeXFrac32Reader
{
    public static Task<Frac32[,]> ParseLatexMatrixAsync(string latex)
    {
        // Удаляем всё, кроме содержимого матрицы и данных
        var cleaned = Regex.Replace(latex, @"\\begin\{.*?\}|\\end\{.*?\}|\s+", "");
        var rows = Regex.Split(cleaned, @"\\\\")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
            
        Console.WriteLine("Regex step done");
            
        if (rows.Length == 0)
            throw new ArgumentException("Invalid matrix format");

        // Разбиваем строки на элементы
        var elements = rows.Select(row => 
                Regex.Split(row, @"(?<!\\)&") // Игнорируем экранированные &
                    .Select(x => x.Replace(@"\&", "&")) // Восстанавливаем &
                    .ToArray())
            .ToList();

        // Определяем размерность матрицы
        int rowCount = elements.Count;
        int colCount = elements[0].Length;
            
        // Проверяем согласованность размеров
        if (elements.Any(r => r.Length != colCount))
            throw new ArgumentException("Inconsistent matrix dimensions");

        Console.WriteLine("Consistence step done");
            
        Frac32[,] matrix = new Frac32[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            // Парсинг основной матрицы
            for (int j = 0; j < colCount; j++)
            {
                if (!TryParseLatexNumber(elements[i][j], out matrix[i, j]))
                    throw new FormatException($"Invalid number format: {elements[i][j]}");
                Console.WriteLine($"step [{i}, {j}]");
            }
        }

        return Task.FromResult(matrix);
    }
    private static bool TryParseLatexNumber(string input, out Frac32 result)
    {
        if (input.Contains("/"))
        {
            string[] frac = input.Split('/');
            result = new Frac32(int.Parse(frac[0]), int.Parse(frac[1]));
        }
        else
        {
            if (!int.TryParse(input, out int converted))
            {
                result = Frac32.Zero;
                return false;
            }

            result = new Frac32(int.Parse(input));
        }
        return true;
    }

    public static Task<Frac32[,]> RevertMatrixAsync(Frac32[,] matrix)
    {
        
    }
    
    public static Task<string> ParseMatrixAsync()
    {
        
    }
}