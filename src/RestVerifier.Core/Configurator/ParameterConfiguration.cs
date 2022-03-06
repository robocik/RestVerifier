using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Core.Configurator;

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

    public object? IgnoreValue { get; set; }

    public bool Ignore => VerifyBehavior == VerifyBehavior.Ignore;
    public string? Name { get; set; }

    public string GetName()
    {
        return Name ?? Parameter.Name;
    }
}