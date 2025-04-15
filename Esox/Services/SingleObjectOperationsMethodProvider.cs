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

    private void WriteSolutionString(string text)
    {
        Model!.MainSystemSolutionFormula += (text + @"\\");
    }

    private Task InitializeSolution()
    {
        WriteSolutionString(@$"\det{{{_writer.Name}}} = " + Matrix.DeterminantFrac32Async(_data));
        return Task.CompletedTask;
    }
}