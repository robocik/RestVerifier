using System;
using System.Linq.Expressions;

namespace RestVerifier.Configurator;

public sealed class Behavior
{
    public static P Verify<P>()
    {
        return default(P);
    }
    public static P Ignore<P>()
    {
        return default(P);
    }

    public static P Transform<P, T>(Expression<Func<P, T>> transform)
    {
        return default(P);
    }
}