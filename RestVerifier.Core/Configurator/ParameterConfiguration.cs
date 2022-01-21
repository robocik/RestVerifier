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

    public bool Ignore => VerifyBehavior == VerifyBehavior.Ignore;

}