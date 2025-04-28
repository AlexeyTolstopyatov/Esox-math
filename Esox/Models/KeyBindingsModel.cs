using System.IO;

namespace Esox.Models;

public class KeyBindingsModel
{
    public KeyBindingsModel(string path)
    {
        string text = File.ReadAllText(path);
        string[] lines = text.Split('\n');
        Building = lines[0];
        Loading = lines[1];
        Saving = lines[2];
        Transposition = lines[3];
        TranspositionFormula = lines[4];
        Replacement = lines[5];
        ReplacementFormula = lines[6];
    }
    
    public string? Building { get; private set; }
    public string? Loading { get; private set; }
    public string? Saving { get; private set; }
    public string? Transposition { get; private set; }
    public string? TranspositionFormula { get; private set; }
    public string? Replacement { get; private set; }
    public string? ReplacementFormula { get; private set; }
}