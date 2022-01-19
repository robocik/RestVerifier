using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace RestVerifier.Configurator;

public class VerifierConfiguation
{


    public Dictionary<Type, Delegate> ReturnTransforms { get; } = new();

    public Dictionary<MethodInfo, MethodConfiguration> Methods { get; } = new();

    public Action<ParameterInfo, ParameterValue>? VerifyParameterAction { get; internal set; }

    public Func<Type, MethodInfo[]> GetMethodFunc { get; internal set; } = (type) =>
    {
        return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(m => !m.IsSpecialName).ToArray();
    };

    public Dictionary<Type, Delegate> ParameterTransforms { get; } = new();

    public Delegate? GetReturnTransform(Type type)
    {
        return GetTransform(ReturnTransforms, type);
    }

    public Delegate? GetParameterTransform(Type type)
    {
        return GetTransform(ParameterTransforms, type);
    }
    private Delegate? GetTransform(Dictionary<Type, Delegate> dict, Type type)
    {
        if (dict.TryGetValue(type, out var transform))
        {
            return transform;
        }
        foreach (var configurationReturnTransform in dict)
        {
            if (configurationReturnTransform.Key.IsAssignableFrom(type))
            {
                return configurationReturnTransform.Value;
            }
        }

        return null;
    }

    public Action<ExecutionContext>? MethodExecuted { get; internal set; } = context =>
    {
        if (context.Exception is not null)
        {
            context.Abort = true;
        }
    };
}