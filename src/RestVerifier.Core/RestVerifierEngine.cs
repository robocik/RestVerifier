﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Net.Http;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;
using RestVerifier.Core.MethodInvokers;

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
        var clientType = typeof(TClient);
        MethodInfo[] methods = _builder.Configuration.GetMethodFunc(clientType);
        return methods;
    }
    public async Task TestService()
    {
        Validator = _builder.CreateValidator();
        var client = await _builder.CreateClient(Validator);
        var methods = GetMethods();

        var methodInvokers = GetMethodInvokers();
        
        for (var index = 0; index < methods.Length; index++)
        {

            var methodInfo = methods[index];
            ExecutionContext context = new ExecutionContext(methodInfo);
            try
            {

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

                await InvokeMethodExecuting(context);

                foreach (var invoker in methodInvokers)
                {
                    await invoker.Invoke(methodInfo,methodConfig, client, context);
                }
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

    private IEnumerable<IMethodInvoker> GetMethodInvokers()
    {
        var list = new List<IMethodInvoker>();
        list.Add(new NormalInvoker(Validator,_builder.Configuration));
        list.Add(new ThrowExceptionInvoker(Validator, _builder.Configuration));
        return list;
    }

    private async Task HandleException(ExecutionContext context, Exception exception, int index, MethodInfo[] methods, MethodInfo methodInfo)
    {
        
        context.Result = ExecutionResult.Error;
        
        if (exception is TargetInvocationException target)
        {
            exception = target.InnerException;
        }

        var listOfExceptions = new List<Exception>();
        listOfExceptions.Add(exception);
        listOfExceptions.AddRange(Validator.Context.Exceptions);

        var newException = new VerifierExecutionException(context.Method,
            $"Execution of {index + 1}/{methods.Length}: {methodInfo.Name} failed", listOfExceptions);
        context.Exception = newException;
        context.Abort = true;
        await InvokeMethodExecuted(context);
        if (context.Abort)
        {
            throw newException;
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
        if (_builder.Configuration.MethodExecuted != null)
        {
            return _builder.Configuration.MethodExecuted(context);
        }

        return Task.CompletedTask;
        
    }

    private Task InvokeMethodExecuting(ExecutionContext context)
    {
        if (_builder.Configuration.MethodExecuting != null)
        {
            return _builder.Configuration.MethodExecuting(context);
        }

        return Task.CompletedTask;
    }

    

    
    
    
}