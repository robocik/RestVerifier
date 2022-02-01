using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.MethodInvokers;

public abstract class MethodInvokerBase:IMethodInvoker
{
    protected MethodInvokerBase(CompareRequestValidator validator, VerifierConfiguration configuration)
    {
        Validator = validator;
        Configuration = configuration;
    }

    protected CompareRequestValidator Validator { get; }

    protected VerifierConfiguration Configuration { get; }

    public abstract Task Invoke(MethodInfo methodInfo, MethodConfiguration methodConfig, object client,
        ExecutionContext context);

    protected async Task<object?> InvokeMethod(MethodInfo methodInfo, object client, object?[] values)
    {

        var result = methodInfo.Invoke(client, values);
        if (result is Task task)
        {
            await task;
            if (methodInfo.ReturnType.IsGenericType)
            {
                result = methodInfo.ReturnType.GetProperty("Result")!.GetValue(task);
            }
            else
            {
                result = null;
            }
        }

        return result;
    }
}