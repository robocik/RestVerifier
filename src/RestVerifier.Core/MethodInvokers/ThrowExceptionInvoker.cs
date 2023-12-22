using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.MethodInvokers;

public class ThrowExceptionInvoker:MethodInvokerBase
{
    public override async Task Invoke(MethodInfo methodInfo, MethodConfiguration methodConfig, object client,
        ExecutionContext context)
    {
        if (methodConfig.ExceptionHandling==ExceptionHandling.Ignore)
        {
            return;
        }
        var paramBuilder = new ParameterBuilder(Configuration, Validator);
        var parameters = methodInfo.GetParameters();
        var values = paramBuilder.AddParameters(methodConfig, parameters);

        foreach (var exception in Configuration.ExceptionsToCheck)
        {
            
            Validator.ExceptionToThrow = exception;
            try
            {
                await InvokeMethod(methodInfo, client, values.ToArray());
                throw new VerifierExecutionException(methodInfo, $"During testing exception handling, we throw an exception of type {exception} but on the client side exception no exception has been thrown. Please check if your client code handle exceptions correctly.");
            }
            catch (Exception e) when (e is not VerifierExecutionException)
            {
                if (e is TargetInvocationException { InnerException: not null } tex)
                {
                    e=tex.InnerException;
                }
                if (e.GetType() == exception || methodConfig.ExceptionHandling==ExceptionHandling.ThrowButDontCheckType)
                {
                    context.Result = ExecutionResult.Success;
                }
                else
                {
                    throw new VerifierExecutionException(methodInfo, $"During testing exception handling, we throw an exception of type {exception} but on the client side exception of type {e.GetType()} has been thrown. Please check if your client code handle exceptions correctly.");
                }
                
            }

        }
    }

    public ThrowExceptionInvoker(CompareRequestValidator validator, VerifierConfiguration configuration) : base(validator, configuration)
    {
    }
}