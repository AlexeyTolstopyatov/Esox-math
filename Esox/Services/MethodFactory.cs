using System;

namespace Esox.Services;

public static class MethodFactory
{
    public class Requirements
    {
        public int Ordinal;
        public bool MakeSingleSolution;
        public bool MakeSingularInstance;
        public bool MakeHomogenousInstance;
        public LinearCastMaker MakerType;
    }
    
    public static IProvider MakeMethodProvider(Requirements reqs)
    {
        if (reqs.MakeSingleSolution && !reqs.MakeSingularInstance)
            return new KramerMethodProvider(reqs.Ordinal);

        return new LinearCastingMethodProvider(
            reqs.Ordinal,
            reqs.MakeSingularInstance,
            reqs.MakeHomogenousInstance,
            reqs.MakerType);
    }
}