﻿using System;
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
        var clientType = client.GetType();
        MethodInfo[] methods = _builder.Configuration.GetMethodFunc(clientType);
        return methods;
    }
    public async Task TestService()
    {
        Validator = _builder.CreateValidator();
        var client = _builder.CreateClient();
        var methods = GetMethods();

        var paramBuilder = new ParameterBuilder(_builder.Configuration, Validator);
        var returnValueBuilder = new ReturnValueBuilder(_builder.Configuration, Validator);
        for (var index = 0; index < methods.Length; index++)
        {

            var methodInfo = methods[index];
            ExecutionContext context = new ExecutionContext(methodInfo, methods);
            try
            {
                if (methodInfo.Name == "ImportDefinitionsFromCsv")
                {

                }

                var methodConfig = GetMethodConfiguration(methodInfo);
                if (methodConfig==null)
                {
                    continue;
                }
                    

                if (methodInfo.IsGenericMethodDefinition)
                {
                    if (methodConfig.GenericParameters.Length != methodInfo.GetGenericArguments().Length)
                    {
                        throw new ArgumentNullException($"Method {methodInfo.Name} is generic. You need to configure it by using Verify or Setup method");
                    }
                    methodInfo = methodInfo.MakeGenericMethod(methodConfig.GenericParameters);
                }
                

                Console.WriteLine("METHOD: " + methodInfo.Name + " - " + index);
                Validator.Reset(methodConfig);

                returnValueBuilder.RegisterClientReturnType(methodConfig,methodInfo.ReturnType);

                ParameterInfo[] parameters = methodInfo.GetParameters();
                IList<object?> values = paramBuilder.AddParameters(methodConfig, parameters);
                
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
                await HandleException(context, e, index, methods, methodInfo);
            }


        }
    }

    private async Task HandleException(ExecutionContext context, Exception exception, int index, MethodInfo[] methods, MethodInfo methodInfo)
    {
        context.Result = ExecutionResult.Error;
        context.Exception = exception;
        if (exception is TargetInvocationException target)
        {
            context.Exception = target.InnerException;
        }

        await InvokeMethodExecuted(context);
        if (context.Abort)
        {
            throw new VerifierExecutionException(context.Method, $"Execution of {index + 1}/{methods.Length}: {methodInfo.Name} failed", context.Exception!);
        }
    }

    private MethodConfiguration? GetMethodConfiguration(MethodInfo methodInfo)
    {
        if (_builder.Configuration.Methods.TryGetValue(methodInfo, out var methodConfig))
        {
            if (methodConfig.Skip)
            {
                return null;
            }
        }
        else if (_builder.Configuration.Mode == EngineMode.Strict)
        {
            //in Strict mode we run only methods which are configured in Verify or Setup section
            return null;
        }

        methodConfig ??= new MethodConfiguration(methodInfo);
        return methodConfig;
    }

    private Task InvokeMethodExecuted(ExecutionContext context)
    {
        return _builder.Configuration.MethodExecuted(context);
    }

    private Task InvokeMethodExecuting(ExecutionContext context)
    {
        return _builder.Configuration.MethodExecuting(context);
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
    
    
}