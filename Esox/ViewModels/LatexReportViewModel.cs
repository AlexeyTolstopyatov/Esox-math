namespace Esox.ViewModels;

public class LatexReportViewModel : NotifyPropertyChanged
{
    #pragma warning disable
    public LatexReportViewModel() { }
    #pragma warning restore
    
    public LatexReportViewModel(string mainSystemFormula, string computesFormulas)
    {
        _computesFormulas = computesFormulas;
        _mainSystemFormula = mainSystemFormula;
    }
    
    private string _computesFormulas;
    private string _mainSystemFormula;

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