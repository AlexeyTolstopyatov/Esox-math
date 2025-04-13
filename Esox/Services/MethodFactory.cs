using System;

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
            reqs.MakeUndefinedInstance);
    }

    /// <summary>
    /// Создает <see cref="IProvider"/> на основе
    /// разметки Latex.
    /// </summary>
    /// <param name="latex"></param>
    /// <returns></returns>
    public static IProvider MakeMethodProvider(string latex)
    {
        throw new NotImplementedException();
    }
}