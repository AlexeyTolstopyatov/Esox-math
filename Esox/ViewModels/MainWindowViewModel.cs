using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Esox.Services;
using Esox.Views;
using Microsoft.Xaml.Behaviors.Core;

namespace Esox.ViewModels;

public class MainWindowViewModel : NotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        _visibility = Visibility.Hidden;
        _makerMode = LinearCastMaker.Triangle;
        _makeCommand = new ActionCommand(Make);
        _fromLatexCommand = new ActionCommand(FromLatex);
    }
    
    #region Private Fields
    // Основная панель -> информация о матрице системы
    private string? _mainSystemFormulaString;
    private string? _detSystemFormulaString;
    private Visibility _visibility;
    // Основная панель -> дополнительная информация
    private Page? _computesPage;

    // Левая панель -> Уточнения для задания матрицы
    private int _mainSystemOrdinal;
    private bool _homogenousSystem;
    private bool _singularSystem;
    private bool _consistentSystem;
    private bool _singleSystemResult;
    // Левая панель -> Установка ограничений
    private bool _allowHomogenousSystem;
    private bool _allowDegenerativeSystem;
    private bool _allowSolutionCharacteristics;
    // Левая панель -> Установка режима генерации
    private LinearCastMaker _makerMode;
    
    #endregion
    
    #region View Bingings
    
    public LinearCastMaker MakerMode
    {
        get => _makerMode;
        set => SetField(ref _makerMode, value);
    }
    public Page? ComputesPage
    {
        get => _computesPage;
        set => SetField(ref _computesPage, value);
    }

    /// <summary>
    /// Система линейных уравнений должна иметь только одно решение
    /// (пересечение всех плоскостей только в одной точке)
    /// </summary>
    public bool SingleSystemResult
    {
        get => _singleSystemResult;
        set => SetField(ref _singleSystemResult, value);
    }
    /// <summary>
    /// Разрешает использовать флаг
    /// единственного решения.
    /// (Выключается обязательно, если система уравнений не совместна)
    /// </summary>
    public bool AllowSolutionCharacteristics
    {
        get => _allowSolutionCharacteristics;
        set => SetField(ref _allowSolutionCharacteristics, value);
    }
    /// <summary>
    /// Разрешить использовать флаг
    /// Однородной расширенной системы линейных уравнений
    /// </summary>
    public bool AllowHomogenousSystem
    {
        get => _allowHomogenousSystem;
        set => SetField(ref _allowHomogenousSystem, value);
    }
    /// <summary>
    /// Разрешить использовать флаг вырожденной
    /// системы линейных уравнений
    /// </summary>
    public bool AllowDegenerativeSystem
    {
        get => _allowDegenerativeSystem;
        set => SetField(ref _allowDegenerativeSystem, value);
    }
    /// <summary>
    /// Однородная система линейных уравнений
    /// </summary>
    public bool HomogenousSystem
    {
        get => _homogenousSystem;
        set => SetField(ref _homogenousSystem, value);
    }
    /// <summary>
    /// Вырожденная система линейных уравнений
    /// </summary>
    public bool DegenerativeSystem
    {
        get => _singularSystem;
        set => SetField(ref _singularSystem, value);
    }
    /// <summary>
    /// Совместная система линейных уравнений
    /// (имеет одно или множество решений)
    /// </summary>
    public bool ConsistentSystem
    {
        get => _consistentSystem;
        set
        {
            SetField(ref _consistentSystem, value);
            AllowDegenerativeSystem = _consistentSystem;
            AllowHomogenousSystem = _consistentSystem;
            AllowSolutionCharacteristics = _consistentSystem;
        }
    }
    /// <summary>
    /// Изменяет видимость основной панели
    /// </summary>
    public Visibility Visibility
    {
        get => _visibility;
        set => SetField(ref _visibility, value);
    }
    /// <summary>
    /// Порядок квадратной матрицы системы
    /// </summary>
    public int MainSystemOrdinal
    {
        get => _mainSystemOrdinal;
        set => SetField(ref _mainSystemOrdinal, value);
    }
    
    #endregion

    #region Pirvate Command storage

    private ICommand _makeCommand;
    private ICommand _fromLatexCommand;

    #endregion

    #region View Command-Bindings
    public ICommand MakeCommand
    {
        get => _makeCommand;
        set => SetField(ref _makeCommand, value);
    }
    public ICommand FromLatexCommand
    {
        get => _fromLatexCommand;
        set => SetField(ref _fromLatexCommand, value);
    }
    #endregion

    private void FromLatex()
    {
        ComputesPage = new SystemMakerView()
        {
            DataContext = new SystemMakerViewModel()
        };
        
        Visibility = Visibility.Visible;
    }
    private void Make()
    {
        MethodFactory.Requirements requirements = new()
        {
            MakeSingleSolution = SingleSystemResult,
            MakeHomogenousInstance = HomogenousSystem,
            MakeSingularInstance = DegenerativeSystem,
            Ordinal = MainSystemOrdinal,
            MakerType = MakerMode
        };
        IProvider method = MethodFactory.MakeMethodProvider(requirements);
        
        ComputesPage = new LatexReportView
        {
            DataContext = new LatexReportViewModel(
                method.Model.MainSystemFormula!,
                method.Model.MainSystemSolutionFormula!)
        };

        Visibility = Visibility.Visible;
    }
}