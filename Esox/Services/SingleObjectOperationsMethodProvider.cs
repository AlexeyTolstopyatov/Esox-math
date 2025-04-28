using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Esox.Models;
using Esox.Types;

namespace Esox.Services;

public class SingleObjectOperationsMethodProvider : IProvider
{
    public CommonMethodComputingModel? Model { get; }
    private LaTeXFrac32Markup _writer;
    private Frac32[,] _data;
    public SingleObjectOperationsMethodProvider(Frac32[,] data)
    {
        Model = new CommonMethodComputingModel();
        _writer = new LaTeXFrac32Markup();
        Model.MainSystemExtendedMatrix = _writer.MakePMatrix(data);
        Model.MainSystemFormula = _writer.MakePMatrix(data);
        _data = data;
        
        _ = InitializeSolution();
    }
    
    protected SingleObjectOperationsMethodProvider()
    {
        
    }

    private void WriteSolutionString(string text)
    {
        Model!.MainSystemSolutionFormula += (text + @"\\");
    }

    private Task InitializeSolution()
    {
        WriteSolutionString(_writer.MakeText("Основные характеристики квадратной матрицы"));
        WriteSolutionString(@$"\det{{({_writer.Name})}} = " + Matrix.DeterminantFrac32Async(_data));

        FindNullSpace(_data);
        return Task.CompletedTask;
    }
    
    /// <summary>
    /// Ищет и записывает ядро квадратной матрицы.
    /// (Важно помнить то, что ядро присуще любой матрицы 
    /// </summary>
    /// <param name="matrix">требуемая матрица системы</param>
    public void FindNullSpace(Frac32[,] matrix)
    {
        int n = matrix.GetLength(0);
        Frac32[,] extended = new Frac32[n, n + 1];

        // расширенная матрицу [A | 0] для решения Ax = 0
        for (int i = 0; i < n; i++)
        {
            for (int j = 0; j < n; j++)
                extended[i, j] = matrix[i, j];
            extended[i, n] = Frac32.Zero; // нулевой столбец
        }

        // приведение к ступенчатому виду
        int[] freedColumns = GaussianElimination(extended);

        // свободные переменные
        List<int> freeVars = new();
        for (int col = 0; col < n; col++)
        {
            if (!freedColumns.Contains(col))
                freeVars.Add(col);
        }

        // Генерация векторов ядра
        List<Frac32[]> nullSpace = new();
        foreach (int freeVar in freeVars)
        {
            Frac32[] vector = new Frac32[n];
            MatrixGenerator.Initialize(ref vector);

            vector[freeVar] = Frac32.Positive;

            for (int row = 0; row < freedColumns.Length; row++)
            {
                int freedCol = freedColumns[row];
                if (freedCol == -1) continue;

                Frac32 sum = Frac32.Zero;
                for (int col = freedCol + 1; col < n; col++)
                    sum += (extended[row, col] * vector[col]);

                vector[freedCol] = (sum * Frac32.Negative) / extended[row, freedCol];
                vector[freedCol].Clear();
            }

            nullSpace.Add(vector);
        }
        
        foreach (var vec in nullSpace)
        {
            Console.WriteLine(string.Join(", ", vec.Select(f => f.ToString())));
        }
        
        // save results
        if (nullSpace.Count == 0)
        {
            // ядро тривиально.
            WriteSolutionString($@"nullity({_writer.Name}) = 0 \Rightarrow \ker({_writer.Name}) = \vec{{0}}");
            return;
        }
        
        string solution =
            $"\\ker({_writer.Name}) = \\alpha_{1} \\cdot" +
            _writer.MakePVectorColumn(nullSpace.ElementAt(0)) + "^T";
        
        // \begin{pmatrix}
        // 1 & 2 & 3 \\ 
        // 3 & 6 & 9 \\
        // 6 & 12 & 18
        // //\end{pmatrix}
        
        if (nullSpace.Count == 1)
            goto __writeAndExit;
        
        for (int i = 1; i < nullSpace.Count; ++i)
        {
            solution +=
                $"+ \\alpha_{{{i + 1}}} \\cdot" +
                _writer.MakePVectorColumn(nullSpace.ElementAt(i)) + "^T" +
                "";
        }
        __writeAndExit:
        solution += ", \\alpha_{n} \\in R";
        WriteSolutionString(solution);
    }

    private static int[] GaussianElimination(Frac32[,] matrix)
    {
        int rows = matrix.GetLength(0);
        int cols = matrix.GetLength(1);
        int[] freedColumns = new int[rows];
        Array.Fill(freedColumns, -1);

        int currentRow = 0;
        for (int col = 0; col < cols - 1 && currentRow < rows; col++)
        {
            int freedRow = -1;
            for (int i = currentRow; i < rows; i++)
            {
                if (!matrix[i, col].Equals(Frac32.Zero))
                {
                    freedRow = i;
                    break;
                }
            }

            if (freedRow == -1) continue;
            
            if (freedRow != currentRow)
            {
                for (int j = col; j < cols; j++)
                {
                    (matrix[currentRow, j], matrix[freedRow, j]) = (matrix[freedRow, j], matrix[currentRow, j]);
                }
            }

            Frac32 pivot = matrix[currentRow, col];
            for (int j = col; j < cols; j++)
            {
                matrix[currentRow, j] /= pivot;
                matrix[currentRow, j].Clear();
            }

            for (int i = 0; i < rows; i++)
            {
                if (i == currentRow) continue;
                Frac32 factor = matrix[i, col];
                for (int j = col; j < cols; j++)
                {
                    matrix[i, j] -= (factor * matrix[currentRow, j]);
                    matrix[i, j].Clear();
                };
            }

            freedColumns[currentRow] = col;
            currentRow++;
        }

        return freedColumns;
    }
}