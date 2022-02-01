using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.MethodInvokers;

public class NormalInvoker: MethodInvokerBase
{
    public override async Task Invoke(MethodInfo methodInfo, MethodConfiguration methodConfig, object client,
        ExecutionContext context)
    {
        var paramBuilder = new ParameterBuilder(Configuration, Validator);
        var parameters = methodInfo.GetParameters();
        IList<object?> values = paramBuilder.AddParameters(methodConfig, parameters);

        var returnObj = await InvokeMethod(methodInfo, client, values.ToArray());

        if (methodInfo.ReturnType.IsVoid())
        {
            returnObj = ValidationContext.NotSet;
        }

        Validator.ThrowIfExceptions();
        Validator.ThrowIfNotReachEndpoint();
        Validator.ValidateReturnValue(returnObj);
        context.Result = ExecutionResult.Success;
    }

    public NormalInvoker(CompareRequestValidator validator, VerifierConfiguration configuration) : base(validator, configuration)
    {
    }
}