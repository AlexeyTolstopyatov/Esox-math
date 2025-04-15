using System;
using Esox.Types;

namespace Esox.Services;

public static class MethodFactory
{
    public class MethodParameters
    {
        public int Ordinal;
        public int Rank;
        public bool KramerMethodRequired;
        public bool MakeConsistent;
        public bool MakeUndefinedInstance;
        public bool MakeHomogenousInstance;
        public LinearCastingGeneratorType GeneratorTypeType;
    }
    
    /// <summary>
    /// Создает <see cref="IProvider"/> на основе
    /// переданных параметров.
    /// </summary>
    /// <param name="reqs">Структура требований</param>
    /// <returns></returns>
    public static IProvider MakeMethodProvider(MethodParameters reqs)
    {
        if (reqs.KramerMethodRequired && 
            !reqs.MakeUndefinedInstance)
            return new KramerMethodProvider(reqs.Ordinal);

        return new LinearCastingMethodProvider(
            reqs.Ordinal,
            reqs.Rank,
            reqs.GeneratorTypeType, 
            reqs.MakeConsistent,
            reqs.MakeUndefinedInstance,
            reqs.MakeHomogenousInstance);
    }

    /// <summary>
    /// Создает <see cref="IProvider"/> на основе
    /// разметки Latex.
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static IProvider MakeMethodProvider(Frac32[,] data)
    {
        if (data.GetLength(0) != data.GetLength(1))
            return new LinearCastingMethodProvider(data);

        return new SingleObjectOperationsMethodProvider(data);
    }
}