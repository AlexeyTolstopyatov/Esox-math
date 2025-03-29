namespace Esox.Views.ViewModels;

public class KramerViewModel : NotifyPropertyChanged
{
    #pragma warning disable
    public KramerViewModel() { }
    #pragma warning restore
    
    public KramerViewModel(string mainSystemFormula, string computesFormulas)
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