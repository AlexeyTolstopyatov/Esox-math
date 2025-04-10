﻿using System;

namespace Esox.Services;

public static class MethodFactory
{
    public class Requirements
    {
        public int Ordinal;
        public bool MakeSingleSolution;
        public bool MakeSingularInstance;
        public bool MakeHomogenousInstance;
        public LinearCastingGeneratorType GeneratorTypeType;
    }
    
    /// <summary>
    /// Создает <see cref="IProvider"/> на основе
    /// переданных параметров.
    /// </summary>
    /// <param name="reqs"></param>
    /// <returns></returns>
    public static IProvider MakeMethodProvider(Requirements reqs)
    {
        if (reqs.MakeSingleSolution && !reqs.MakeSingularInstance)
            return new KramerMethodProvider(reqs.Ordinal);

        return new LinearCastingMethodProvider(
            reqs.Ordinal, reqs.GeneratorTypeType);
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