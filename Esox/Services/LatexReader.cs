using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Esox.Types;

namespace Esox.Services;

public static class LatexReader
{
    public static Task<Frac32[,]> ParseLatexMatrixAsync(string latex)
    {
        var cleaned = Regex.Replace(latex, @"\\begin\{.*?\}|\\end\{.*?\}|\s+", "");
        var rows = Regex.Split(cleaned, @"\\\\")
            .Where(x => !string.IsNullOrEmpty(x))
            .ToArray();
            
        if (rows.Length == 0)
            throw new ArgumentException("Invalid matrix format");

        var elements = rows.Select(row => 
                Regex.Split(row, @"(?<!\\)&") 
                    .Select(x => x.Replace(@"\&", "&"))
                    .ToArray())
            .ToList();

        int rowCount = elements.Count;
        int colCount = elements[0].Length;
            
        if (elements.Any(r => r.Length != colCount))
            throw new ArgumentException("Не совместимые измерения");

        Frac32[,] matrix = new Frac32[rowCount, colCount];

        for (int i = 0; i < rowCount; i++)
        {
            for (int j = 0; j < colCount; j++)
            {
                if (!TryParseLatexNumber(elements[i][j], out matrix[i, j]))
                    throw new FormatException($"Неправильный формат элемента: {elements[i][j]}.");
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
            if (!int.TryParse(input, out int _))
            {
                result = Frac32.Zero;
                return false;
            }

            result = new Frac32(int.Parse(input));
        }
        return true;
    }
    
    public static string TransposeLatexMatrix(string latexMatrix)
    {
        Match matrixMatch = Regex.Match(latexMatrix, @"\\begin{(.*?matrix)}(.*?)\\end{(.*?matrix)}", RegexOptions.Singleline);

        if (!matrixMatch.Success)
            throw new ArgumentException("Некорректный формат LaTeX-матрицы");

        string matrixType = matrixMatch.Groups[1].Value;
        string matrixContent = matrixMatch.Groups[2].Value.Trim();

        string[] rows = matrixContent.Split(new[] { "\\\\" }, StringSplitOptions.RemoveEmptyEntries);
        for (int i = 0; i < rows.Length; i++)
            rows[i] = rows[i].Trim();

        string[][] matrixElements = new string[rows.Length][];
        for (int i = 0; i < rows.Length; i++)
        {
            matrixElements[i] = rows[i].Split(new[] { "&" }, StringSplitOptions.RemoveEmptyEntries);
            for (int j = 0; j < matrixElements[i].Length; j++)
                matrixElements[i][j] = matrixElements[i][j].Trim();
        }

        string[][] transposedElements = new string[matrixElements[0].Length][];
        for (int i = 0; i < matrixElements[0].Length; i++)
        {
            transposedElements[i] = new string[matrixElements.Length];
            for (int j = 0; j < matrixElements.Length; j++)
            {
                transposedElements[i][j] = matrixElements[j][i];
            }
        }

        string[] transposedRows = new string[transposedElements.Length];
        for (int i = 0; i < transposedElements.Length; i++)
        {
            transposedRows[i] = string.Join(" & ", transposedElements[i]);
        }

        string transposedContent = string.Join(" \\\\\n", transposedRows);
        string transposedLatex = $"\\begin{{{matrixType}}}\n{transposedContent}\n\\end{{{matrixType}}}";

        return transposedLatex;
    }

    public static string ReplaceMacro(string latexMatrix)
    {
        // Регулярное выражение для поиска \frac{числитель}{знаменатель}
        string pattern = @"\\frac{([^{}]+)}{([^{}]+)}";

        // Замена на "числитель/знаменатель"
        string result = Regex.Replace(latexMatrix, pattern, @"$1/$2");

        return result;
    }
}