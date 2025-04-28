using System;
using Esox.Models;

namespace Esox.ViewModels;

public class KeyBindingsViewModel
{
    public KeyBindingsModel Model { get; } =
        new(AppDomain.CurrentDomain.BaseDirectory + "Esox.Interface.txt");
}