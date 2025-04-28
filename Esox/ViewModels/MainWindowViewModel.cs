using System;
using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Esox.Services;
using Esox.Views;
using Microsoft.Xaml.Behaviors.Core;
using MessageBox = HandyControl.Controls.MessageBox;

namespace Esox.ViewModels;

public class MainWindowViewModel : NotifyPropertyChanged
{
    public MainWindowViewModel()
    {
        _generatorTypeMode = LinearCastingGeneratorType.Triangle;
        _makeCommand = new ActionCommand(Make);
        _fromLatexCommand = new ActionCommand(FromLatex);
        _saveLatexCommand = new ActionCommand(Save);
        _helpCommand = new ActionCommand(Help);
        _transposeCommand = new ActionCommand(Transpose);
        _replaceMacroCommand = new ActionCommand(ReplaceMacro);
        _aboutMessage = string.Empty;
        _versionMessage = string.Empty;
        _aboutMessage = string.Empty;
        _latexFormula = string.Empty;
        _environmentHelpMessage = string.Empty;
        _environmentHelpMessage = string.Empty;
        
        InitializeTextBlocks();
    }
    
    private void InitializeTextBlocks()
    {
        ComputesPage = new TheoryView()
        {
            DataContext = new TheoryViewModel()
        };
        _environmentHelpMessage = @"Используйте 'pmatrix' окружение для обозначения матрицы. 
1) Указывайте '\begin{pmatrix}' и '\end{pmatrix}' области использования или используйте
сокращение встренное в ПО: 

\pmatrix{ матрица системы здесь }

2) Вместо дробей LaTeX - \frac{x}{y}, используйте линейный вид: 'x/y'

3) Формула должна содержать только цело-численные значения или значения в виде обыкновенных дробей

Если вместо формулы вы видите красную полосу - скорее всего, формула содержит ошибки.";
        _aboutMessage = @"Esox генерирует и решает системы линейных алгебраических уравнений порядка от 2 до 10. Поддерживает системные цвета, если операционная система - Windows 10 и выше.";
        _latexFormula = @"\begin{pmatrix}
2 & 3 & 3 \\ 
2 & 1 & 0
\end{pmatrix}";
        _versionMessage = FileVersionInfo.GetVersionInfo(Assembly.GetExecutingAssembly().Location).FileVersion!;
    }
    
    #region Private Fields
    // Основная панель -> дополнительная информация
    private Visibility _navigationVisibility;
    private Visibility _visibility;
    private Page? _computesPage;

    // Левая панель -> Уточнения для задания матрицы
    private int _mainSystemOrdinal;
    private bool _homogenousSystem;
    private bool _undefinedSystem;
    private bool _consistentSystem;
    private bool _kramerMethodFlag;
    
    // Левая панель -> Установка ограничений
    private bool _allowHomogenousSystem;
    private bool _allowUndefinedSystem;
    private bool _allowSolutionCharacteristics;
    private bool _allowGenerationMethods;
    
    // Левая панель -> Установка режима генерации
    private LinearCastingGeneratorType _generatorTypeMode;
    private int _mainSystemRequiredRank;
    
    #endregion
    
    // Левая панель -> Создание своей системы уравнений
    private string _latexFormula;
    private string _environmentHelpMessage;
    
    // Левая панель -> About
    private string _aboutMessage;
    private string _versionMessage;
    
    #region View Bingings
    public string AboutMessage
    {
        get => _aboutMessage;
        set => SetField(ref _aboutMessage, value);
    }

    public string VersionMessage
    {
        get => _versionMessage;
        set => SetField(ref _versionMessage, value);
    }
    public string EnvironmentHelpMessage
    {
        get => _environmentHelpMessage;
        set => SetField(ref _environmentHelpMessage, value);
    }
    public string LatexFormula
    {
        get => _latexFormula;
        set => SetField(ref _latexFormula, value);
    }
    public LinearCastingGeneratorType GeneratorTypeMode
    {
        get => _generatorTypeMode;
        set => SetField(ref _generatorTypeMode, value);
    }
    public Page? ComputesPage
    {
        get => _computesPage;
        private set => SetField(ref _computesPage, value);
    }

    /// <summary>
    /// Система линейных уравнений должна иметь только одно решение
    /// (пересечение всех плоскостей только в одной точке)
    /// </summary>
    public bool KramerMethodFlag
    {
        get => _kramerMethodFlag;
        set => SetField(ref _kramerMethodFlag, value);
    }

    /// <summary>
    /// Если хоть раз была создана страница решения,
    /// модель данных не может быть пустой
    /// </summary>
    public bool ModelNotNull => 
        ComputesPage is LatexReportView;

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
    public bool AllowUndefinedSystem
    {
        get => _allowUndefinedSystem;
        set => SetField(ref _allowUndefinedSystem, value);
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
    /// Неопределенная система линейных уравнений
    /// </summary>
    public bool UndefinedSystem
    {
        get => _undefinedSystem;
        set
        {
            AllowGenerationMethods = _undefinedSystem;
            SetField(ref _undefinedSystem, value);
        }
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
            AllowUndefinedSystem = _consistentSystem;
            AllowHomogenousSystem = _consistentSystem;
            AllowSolutionCharacteristics = _consistentSystem;
            AllowGenerationMethods = _consistentSystem;
        }
    }
    public bool AllowGenerationMethods
    {
        get => _allowGenerationMethods;
        set => SetField(ref _allowGenerationMethods, value);
    }
    /// <summary>
    /// Изменяет видимость основной панели
    /// </summary>
    public Visibility Visibility
    {
        get => _visibility;
        set => SetField(ref _visibility, value);
    }

    public Visibility NavigationVisibility
    {
        get => _visibility;
        set => SetField(ref _navigationVisibility, value);
    }
    /// <summary>
    /// Порядок квадратной матрицы системы
    /// </summary>
    public int MainSystemOrdinal
    {
        get => _mainSystemOrdinal;
        set => SetField(ref _mainSystemOrdinal, value);
    }

    public int MainSystemRequiredRank
    {
        get => _mainSystemRequiredRank;
        set => SetField(ref _mainSystemRequiredRank, value);
    }
    #endregion

    #region Pirvate Command storage
    private ICommand _makeCommand;
    private ICommand _fromLatexCommand;
    private ICommand _saveLatexCommand;
    private ICommand _helpCommand;
    private ICommand _transposeCommand;
    private ICommand _replaceMacroCommand;
    #endregion

    #region View Command-Bindings

    public ICommand ReplaceMacroCommand
    {
        get => _replaceMacroCommand;
        set => SetField(ref _replaceMacroCommand, value);
    }
    public ICommand TransposeCommand
    {
        get => _transposeCommand;
        set => SetField(ref _transposeCommand, value);
    }
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

    public ICommand SaveLatexCommand
    {
        get => _saveLatexCommand;
        set => SetField(ref _saveLatexCommand, value);
    }

    public ICommand HelpCommand
    {
        get => _helpCommand;
        set => SetField(ref _helpCommand, value);
    }
    #endregion

    private void FromLatex()
    {
        try
        {
            IProvider method = MethodFactory
                .MakeMethodProvider(LatexReader.ParseLatexMatrixAsync(LatexFormula).Result);

            ComputesPage = new LatexReportView
            {
                DataContext = new LatexReportViewModel(
                    method.Model!)
            };
        }
        catch (Exception e)
        {
            MessageBox.Warning(e.Message);
        }
        
        NavigationVisibility = Visibility.Visible;
        Visibility = Visibility.Visible;
    }
    private void Make()
    {
        MethodFactory.MethodParameters methodParameters = new()
        {
            KramerMethodRequired = KramerMethodFlag,
            MakeConsistent = ConsistentSystem,
            MakeHomogenousInstance = HomogenousSystem,
            MakeUndefinedInstance = UndefinedSystem,
            Ordinal = MainSystemOrdinal,
            Rank = MainSystemRequiredRank,
            GeneratorTypeType = GeneratorTypeMode
        };
        IProvider method = MethodFactory.MakeMethodProvider(methodParameters);

        ComputesPage = new LatexReportView
        {
            DataContext = new LatexReportViewModel(method.Model!)
        };
        NavigationVisibility = Visibility.Visible;
        Visibility = Visibility.Visible;
    }

    private void Save()
    {
        if (ComputesPage is LatexReportView { DataContext: LatexReportViewModel vm })
        {
            LatexFormula = vm.MainSystemExtendedMatrix;
        }
    }

    private void ReplaceMacro()
    {
        LatexFormula = LatexReader.ReplaceMacro(LatexFormula);
    }
    private void Transpose()
    {
        // matrix transposition [Ctrl + T]
        LatexFormula = LatexReader.TransposeLatexMatrix(LatexFormula);
    }
    
    private void Help()
    {
        NavigationVisibility = Visibility.Hidden;
        
        ComputesPage = new KeyBindingsView()
        {
            DataContext = new KeyBindingsViewModel()
        };
        Visibility = Visibility.Visible;
    }
}