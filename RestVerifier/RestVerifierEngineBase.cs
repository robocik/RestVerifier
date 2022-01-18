using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifier;



public class RestVerifierEngineBase<TClient>
{
    private readonly VerifierConfigurationBuilder<TClient> _builder;

    protected internal RestVerifierEngineBase(VerifierConfigurationBuilder<TClient> builder)
    {
        _builder = builder;
    }

    public static IGlobalSetupStarter<TClient> Create()
    {
        var builder = new VerifierConfigurationBuilder<TClient>();
        return builder;
    }

    protected CompareRequestValidator Validator { get; private set; } = null!;

    public async Task TestService()
    {
        Validator = _builder.CreateValidator();
        await Invoke(async client =>
        {
            var clientType = client.GetType();
            MethodInfo[] methods = _builder.Configuation.GetMethodFunc(clientType);
            for (var index = 0; index < methods.Length; index++)
            {
                var methodInfo = methods[index];
                try
                {
                    if (methodInfo.Name == "ImportDefinitionsFromCsv")
                    {

                    }
                    if (_builder.Configuation.Methods.TryGetValue(methodInfo, out var methodConfig))
                    {
                        if (methodConfig.Skip)
                        {
                            continue;
                        }
                    }
                    ParameterInfo[] parameters = methodInfo.GetParameters();

                    Console.WriteLine("METHOD: " + methodInfo.Name + " - " + index);
                    Validator.Reset(methodConfig);

                    Validator.RegisterClientReturnType(methodInfo.ReturnType);

                    IList<object?> values = AddParameters(methodInfo, methodConfig, parameters);

                    var returnObj = await InvokeMethod(methodInfo, client, values.ToArray());
                    if (methodInfo.Name == "GetCheckedOutRevisionComment")
                    {

                    }
                    Validator.ValidateReturnValue(returnObj);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
                }


            }
        });

    }

    

    

    private IList<object?> AddParameters(MethodInfo method, MethodConfiguration? methodConfig, ParameterInfo[] parameters)
    {
        var list = new List<ParameterValue>();
        if (method.Name == "GetInternalIndices")
        {

        }
        foreach (var parameterInfo in parameters)
        {

            var paramValue = new ParameterValue(parameterInfo.Name!);
            ParameterConfiguration? paramConfig = null;
            if (methodConfig?.Parameters.TryGetValue(parameterInfo, out paramConfig) == true)
            {
                paramValue.Ignore = paramConfig!.Ignore;
            }
            paramValue.Value = EvaluateInitialValue(paramConfig,  parameterInfo);

            paramValue.ValueToCompare=EvaluateVerifyValue(paramConfig, paramValue);
            _builder.Configuation.VerifyParameterAction?.Invoke(parameterInfo,paramValue);
            list.Add(paramValue);


        }

        object?[] arr;
        if (methodConfig?.Transform != null)
        {
            arr = (object[])methodConfig.Transform.DynamicInvoke(list.Select(x => x.ValueToCompare).ToArray())!;
        }
        else
        {
            arr = list.Where(x => !x.Ignore).Select(x => x.ValueToCompare).ToArray();
        }
        Validator.Context.AddParameters(list.ToArray());
        Validator.Context.AddValues(arr);
        return list.Select(x => x.Value).ToList();
    }

    private static object? EvaluateVerifyValue(ParameterConfiguration? paramConfig,ParameterValue paramValue)
    {
        var value = paramValue.Value;
        if (paramConfig?.VerifyExpression != null)
        {
            if (paramConfig!.VerifyExpression is MethodCallExpression mce)
            {
                if (mce.Method.DeclaringType == typeof(Behavior))
                {

                    if (mce.Method.Name == nameof(Behavior.Transform))
                    {
                        UnaryExpression expression = (UnaryExpression)mce.Arguments[0];
                        var exp1 = (LambdaExpression)expression.Operand;
                        value  = exp1.Compile().DynamicInvoke(value);
                    }
                }
                else
                {
                    value = Expression.Lambda(paramConfig.VerifyExpression).Compile().DynamicInvoke();

                }
            }
            else
            {
                value = Expression.Lambda(paramConfig.VerifyExpression).Compile().DynamicInvoke();

            }
        }

        return value;
    }

    private object? EvaluateInitialValue(ParameterConfiguration? paramConfig, ParameterInfo parameterInfo)
    {
        object? value = null;
        if (paramConfig?.SetupExpression != null)
        {
            if (paramConfig!.SetupExpression is MethodCallExpression mce)
            {
                if (mce.Method.DeclaringType == typeof(Data))
                {
                    if (mce.Method.Name == nameof(Data.Generate))
                    {
                    }
                }
                else
                {
                    var yu = Expression.Lambda(paramConfig.SetupExpression).Compile().DynamicInvoke();
                    //var yu = ne.Constructor!.Invoke(ne.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray());
                    value = yu;
                }
            }
            else
            {
                var yu = Expression.Lambda(paramConfig.SetupExpression).Compile().DynamicInvoke();
                //var yu = ne.Constructor!.Invoke(ne.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray());
                value = yu;
            }
        }

        if (value == null)
        {
            value = Validator.Creator.Create(parameterInfo.ParameterType);
        }

        return value;
    }


    protected virtual async Task<object?> InvokeMethod(MethodInfo methodInfo, object client, object?[] values)
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


    private  Task Invoke(Func<TClient, Task> action)
    {
        var client = _builder.CreateClient();
        return action(client);
    }

    
}