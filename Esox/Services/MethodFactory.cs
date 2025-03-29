using System;

namespace Esox.Services;

public static class MethodFactory
{
    public class Requirements
    {
        public int Ordinal;
        public bool MakeSingleSolution;
        public bool MakeDegenerateInstance;
        public bool MakeHomogenousInstance;
    }
    
    public static IProvider MakeMethodProvider(Requirements reqs)
    {
        if (reqs.MakeSingleSolution && !reqs.MakeDegenerateInstance)
            return new KramerMethodProvider(reqs.Ordinal);

        if (reqs.MakeDegenerateInstance)
            return new LinearCastingMethodProvider(
                reqs.Ordinal,
                reqs.MakeDegenerateInstance,
                reqs.MakeHomogenousInstance);
        
        throw new NotImplementedException();
    }
}