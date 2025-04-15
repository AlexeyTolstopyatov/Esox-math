﻿using System.Windows;
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
        _generatorTypeMode = LinearCastingGeneratorType.Triangle;
        _makeCommand = new ActionCommand(Make);
        _fromLatexCommand = new ActionCommand(FromLatex);
        _environmentHelpMessage = @"Используйте 'pmatrix' окружение для обозначения матрицы. 
1) Указывайте '\begin{pmatrix}' и '\end{pmatrix}' области использования или используйте
сокращение встренное в ПО: 

\pmatrix{ матрица системы здесь }

2) Вместо дробей LaTeX - \frac{x}{y}, используйте линейный вид: 'x/y'

3) Формула должна содержать только цело-численные значения или значения в виде обыкновенных дробей

Если вместо формулы вы видите красную полосу - скорее всего, формула содержит ошибки.";
        _visibility = Visibility.Hidden;
        _computesPage = new Page();
        _latexFormula = @"\pmatrix{2 & 3 & 3 \\ 2 & 1 & 0}";
    }
    
    #region Private Fields
    // Основная панель -> дополнительная информация
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

    #region View Bingings
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
        IProvider method = MethodFactory
            .MakeMethodProvider(LaTeXFrac32Reader.ParseLatexMatrixAsync(LatexFormula).Result);
        
        ComputesPage = new LatexReportView // Null-Reference conflict
        {
            DataContext = new LatexReportViewModel(
                method.Model!.MainSystemFormula!,
                method.Model.MainSystemSolutionFormula!)
        };
        
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
            DataContext = new LatexReportViewModel(
                method.Model!.MainSystemFormula!,
                method.Model.MainSystemSolutionFormula!)
        };
        
        Visibility = Visibility.Visible;
    }
}