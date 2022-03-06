using System;
using System.Linq.Expressions;

namespace RestVerifier.Core.Configurator;

public sealed class Behavior
{
    public static T Verify<T>(T ignoreValue)
    {
        return default(T)!;
    }
    public static T Verify<T>(T ignoreValue, string actionParameterName )
    {
        return default(T)!;
    }
    public static T Verify<T>(string actionParameterName)
    {
        return default(T)!;
    }
    public static T Verify<T>()
    {
        return default(T)!;
    }
    public static T Ignore<T>()
    {
        return default(T)!;
    }

    public static T Transform<T>(Expression<Func<T, object>> transform)
    {
        return default(T)!;
    }

    public static T Generate<T>()
    {
        return default(T)!;
    }
}