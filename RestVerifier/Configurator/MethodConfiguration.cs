using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Configurator;

public class ParameterConfiguration
{
    public ParameterConfiguration(ParameterInfo parameter)
    {
        Parameter = parameter;
    }

    public ParameterInfo Parameter { get; }
    public Expression? VerifyExpression { get; set; }
    public Expression? SetupExpression { get; set; }

    public bool Ignore { get; set; }

}
public class MethodConfiguration
{
    public bool Skip { get; set; }
    public Dictionary<ParameterInfo, ParameterConfiguration> Parameters { get; } = new();
    public Delegate? Transform { get; set; }
    public Delegate? ReturnTransform { get; set; }
}