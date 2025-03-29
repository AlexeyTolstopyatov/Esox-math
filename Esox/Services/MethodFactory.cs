using System;

namespace Esox.Services;

public static class MethodFactory
{
    public class Requirements
    {
        public int Ordinal;
        public bool MakeSingleSolution;
        public bool MakeDegenerativeInstance;
        public bool MakeHomogenousInstance;
    }
    
    public static IProviderService MakeMethodProvider(Requirements reqs)
    {
        if (reqs.MakeSingleSolution && !reqs.MakeDegenerativeInstance)
            return new KramerMethodProvider(reqs.Ordinal);

        
        throw new NotImplementedException();
    }
}