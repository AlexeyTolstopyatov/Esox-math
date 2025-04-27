using System.IO;
using System.Threading.Tasks;
using System.Windows.Annotations;

namespace Esox.Models;

public class TheoryModel
{
    public TheoryModel(string path)
    {
        FillAsync(path);
    }

    private void FillAsync(string path)
    {
        if (!File.Exists(path)) return;
        
        string data = File.ReadAllText(path);
        string[] lines = data.Split('\n');

        Ordinal = lines[0];
        OrdinalFormula = lines[1];
        Rank = lines[2];
        RankFormula = lines[3];
        Determinant = lines[4];
        DeterminantFormula = lines[5];
        Kernel = lines[6];
        KernelFormula = lines[7];
    }
    
    public string? Ordinal { get; private set; }

    public string? OrdinalFormula { get; private set; }

    public string? Rank { get; private set; }

    public string? RankFormula { get; private set; }

    public string? Determinant { get; private set; }

    public string? DeterminantFormula { get; private set; }

    public string? Kernel { get; private set; }

    public string? KernelFormula { get; private set; }
}