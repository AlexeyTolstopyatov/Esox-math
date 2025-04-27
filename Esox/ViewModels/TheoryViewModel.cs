using System;
using Esox.Models;

namespace Esox.ViewModels;

public class TheoryViewModel
{
    public TheoryModel Model { get; } = 
        new(AppDomain.CurrentDomain.BaseDirectory + "Esox.Data.txt");
}