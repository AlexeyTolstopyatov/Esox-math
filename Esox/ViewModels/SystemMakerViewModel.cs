using System.Windows;
using System.Windows.Input;
using Esox.Services;
using Microsoft.Xaml.Behaviors.Core;

namespace Esox.ViewModels;

public class SystemMakerViewModel : NotifyPropertyChanged
{
    public SystemMakerViewModel()
    {
        _parseMatrixCommand = new ActionCommand(ParseMatrix);
        _parseAvoidCheckCommand = new ActionCommand(ParseAvoidCheck);
    }
    
    public string? Matrix
    {
        get => _matrix;
        set => SetField(ref _matrix, value);
    }
    public string? Text
    {
        get => _text;
        set => SetField(ref _text, value);
    }

    public ICommand ParseMatrixCommand
    {
        get => _parseMatrixCommand;
        set => SetField(ref _parseMatrixCommand, value);
    }

    public ICommand ParseAvoidCheckCommand
    {
        get => _parseAvoidCheckCommand;
        set => SetField(ref _parseAvoidCheckCommand, value);
    }
    
    private string? _text;
    private string? _matrix;

    private ICommand _parseMatrixCommand;
    private ICommand _parseAvoidCheckCommand;
    private void ParseMatrix()
    {
        
    }

    private void ParseAvoidCheck()
    {
        IProvider method = MethodFactory.MakeMethodProvider(_text!);
        Matrix = method.Model.MainSystemFormula;
    }
}