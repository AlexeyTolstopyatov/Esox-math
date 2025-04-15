using Esox.Models;

namespace Esox.ViewModels;

public class LatexReportViewModel : NotifyPropertyChanged
{
    #pragma warning disable
    public LatexReportViewModel() { }
    #pragma warning restore
    
    public LatexReportViewModel(CommonMethodComputingModel model)
    {
        _mainSystemFormula = model.MainSystemFormula!;
        _computesFormulas = model.MainSystemSolutionFormula!;
        MainSystemExtendedMatrix = model.MainSystemExtendedMatrix!;
    }
    
    private string _computesFormulas;
    private string _mainSystemFormula;

    public string MainSystemExtendedMatrix
    {
        get;
        private set;
    }
    
    public string ComputesFormulas
    {
        get => _computesFormulas;
        set => SetField(ref _computesFormulas, value);
    }

    public string MainSystemFormula
    {
        get => _mainSystemFormula;
        set => SetField(ref _mainSystemFormula, value);
    }
}