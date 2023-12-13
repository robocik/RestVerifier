using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Core.Configurator;

public enum VerifyBehavior
{
    Default,
    GenerateValue,
    Ignore,
    Transform
}

public enum ExceptionHandling
{
    ThrowAndCheckType,
    ThrowButDontCheckType,
    Ignore
}
public sealed class MethodConfiguration
{
    public MethodConfiguration(MethodInfo methodInfo)
    {
        MethodInfo = methodInfo;
        ReturnType = methodInfo.ReturnType.GetTypeWithoutTask();
    }

    public MethodInfo MethodInfo { get; }
    public bool Skip { get; set; }
    public Dictionary<ParameterInfo, ParameterConfiguration> Parameters { get; } = new();
    public Delegate? Transform { get; set; }
    public Delegate? ReturnTransform { get; set; }

    public Type[] GenericParameters { get; internal set; } = Array.Empty<Type>();
    public Type? ReturnType { get; internal set; }
    public bool ShouldNotReturnValue { get; set; }
    public ExceptionHandling ExceptionHandling { get; set; }= ExceptionHandling.ThrowAndCheckType;
}