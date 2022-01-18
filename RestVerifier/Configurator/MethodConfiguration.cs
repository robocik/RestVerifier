using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Configurator;

public enum VerifyBehavior
{
    Default,
    GenerateValue,
    Ignore,
    Transform
}
public class ParameterConfiguration
{
    public ParameterConfiguration(ParameterInfo parameter)
    {
        Parameter = parameter;
    }

    public ParameterInfo Parameter { get; }
    public Expression? VerifyExpression { get; set; }
    public Expression? SetupExpression { get; set; }

    public VerifyBehavior VerifyBehavior { get; set; } = VerifyBehavior.Default;

    public bool Ignore => VerifyBehavior == VerifyBehavior.Ignore;

}
public class MethodConfiguration
{
    public bool Skip { get; set; }
    public Dictionary<ParameterInfo, ParameterConfiguration> Parameters { get; } = new();
    public Delegate? Transform { get; set; }
    public Delegate? ReturnTransform { get; set; }
}