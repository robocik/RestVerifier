using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core;



public class RestVerifierEngine<TClient> where TClient: notnull
{
    private readonly VerifierConfigurationBuilder<TClient> _builder;

    protected internal RestVerifierEngine(VerifierConfigurationBuilder<TClient> builder)
    {
        _builder = builder;
    }

    public static IGlobalSetupStarter<TClient> Create()
    {
        var builder = new VerifierConfigurationBuilder<TClient>();
        return builder;
    }

    protected CompareRequestValidator Validator { get; private set; } = null!;

    public MethodInfo[] GetMethods()
    {
        var client = _builder.CreateClient();
        var clientType = client!.GetType();
        MethodInfo[] methods = _builder.Configuration.GetMethodFunc(clientType);
        return methods;
    }
    public async Task TestService()
    {
        Validator = _builder.CreateValidator();
        var client = _builder.CreateClient();
        var methods = GetMethods();
        for (var index = 0; index < methods.Length; index++)
        {

            var methodInfo = methods[index];
            var context = new ExecutionContext(methodInfo, methods);
            try
            {
                if (methodInfo.Name == "ImportDefinitionsFromCsv")
                {

                }
                if (_builder.Configuration.Methods.TryGetValue(methodInfo, out var methodConfig))
                {
                    if (methodConfig.Skip)
                    {
                        continue;
                    }
                }
                else if (_builder.Configuration.Mode == EngineMode.Strict)
                {//in Strict mode we run only methods which are configured in Verify or Setup section
                    continue;
                }

                if (methodInfo.IsGenericMethodDefinition)
                {
                    if (methodConfig?.GenericParameters.Length != methodInfo.GetGenericArguments().Length)
                    {
                        throw new ArgumentNullException($"Method {methodInfo.Name} is generic. You need to configure it by using Verify or Setup method");
                    }
                    methodInfo = methodInfo.MakeGenericMethod(methodConfig.GenericParameters);
                }
                ParameterInfo[] parameters = methodInfo.GetParameters();

                Console.WriteLine("METHOD: " + methodInfo.Name + " - " + index);
                Validator.Reset(methodConfig);

                Validator.RegisterClientReturnType(methodInfo.ReturnType);

                IList<object?> values = AddParameters(methodInfo, methodConfig, parameters);

                await InvokeMethodExecuting(context);
                if (context.Abort)
                {
                    return;
                }
                var returnObj = await InvokeMethod(methodInfo, client, values.ToArray());
                if (methodInfo.Name == "GetPdfFileWithCustomHeader")
                {

                }
                Validator.ValidateReturnValue(returnObj);
                context.Result = ExecutionResult.Success;
                await InvokeMethodExecuted(context);
                if (context.Abort)
                {
                    return;
                }

            }
            catch (Exception e)
            {
                context.Result = ExecutionResult.Error;
                if (e is TargetInvocationException target)
                {
                    context.Exception = target.InnerException;
                }
                else
                {
                    context.Exception =e;
                }
                
                await InvokeMethodExecuted(context);
                if (context.Abort)
                {
                    throw new VerifierExecutionException(context.Method, $"Execution of {index+1}/{methods.Length}: {methodInfo.Name} failed", context.Exception!);
                }
            }


        }
    }

    private Task InvokeMethodExecuted(ExecutionContext context)
    {
        return _builder.Configuration.MethodExecuted(context);
    }

    private Task InvokeMethodExecuting(ExecutionContext context)
    {
        return _builder.Configuration.MethodExecuting(context);
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
                paramValue.Ignore = paramConfig!.VerifyBehavior==VerifyBehavior.Ignore;
            }
            paramValue.Value = EvaluateInitialValue(paramConfig,  parameterInfo);

            paramValue.ValueToCompare=EvaluateVerifyValue(paramConfig, paramValue);
            _builder.Configuration.VerifyParameterAction?.Invoke(parameterInfo,paramValue);
            list.Add(paramValue);


        }

        object?[] arr;
        if (methodConfig?.Transform != null)
        {
            object?[] valuesArray = list.Select(x => x.ValueToCompare).ToArray();
            var methodParams = methodConfig.Transform.Method.GetParameters();
            if (methodParams.Length == 1 && methodParams[0].ParameterType == typeof(object?[]))
            {
                arr = (object[])methodConfig.Transform.DynamicInvoke(new[] { valuesArray })!;
            }
            else
            {
                arr = (object[])methodConfig.Transform.DynamicInvoke(valuesArray)!;
            }
            
        }
        else
        {
            arr = list.Where(x => !x.Ignore).Select(x => x.ValueToCompare).ToArray();
        }
        Validator.Context.AddParameters(list.ToArray());
        Validator.Context.AddValues(arr);
        return list.Select(x => x.Value).ToList();
    }

    private object? EvaluateVerifyValue(ParameterConfiguration? paramConfig,ParameterValue paramValue)
    {
        var value = paramValue.Value;

        var type=paramConfig?.Parameter.ParameterType??value?.GetType();
        if (type != null)
        {
            var transform = _builder.Configuration.GetParameterTransform(type);
            if (transform != null)
            {
                value = (object?)transform.DynamicInvoke(value);
            }
        }

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
        else if (paramConfig?.VerifyBehavior == VerifyBehavior.GenerateValue)
        {
            value = Validator.Creator.Create(paramConfig.Parameter.ParameterType);
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
                var yu = Expression.Lambda(paramConfig.SetupExpression).Compile().DynamicInvoke();
                value = yu;
            }
            else
            {
                var yu = Expression.Lambda(paramConfig.SetupExpression).Compile().DynamicInvoke();
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
        // if (methodInfo.IsGenericMethod)
        // {
        //     var types=methodInfo.GetGenericArguments();
        //     methodInfo=methodInfo.MakeGenericMethod(types);
        // }
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