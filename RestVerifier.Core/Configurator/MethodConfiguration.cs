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

public class MethodConfiguration
{
    public bool Skip { get; set; }
    public Dictionary<ParameterInfo, ParameterConfiguration> Parameters { get; } = new();
    public Delegate? Transform { get; set; }
    public Delegate? ReturnTransform { get; set; }

    public Type[] GenericParameters { get; internal set; } = Array.Empty<Type>();
}