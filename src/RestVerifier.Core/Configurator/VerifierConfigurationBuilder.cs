﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;
using RestVerifier.Core.Matchers;

namespace RestVerifier.Core.Configurator;


public sealed class VerifierConfigurationBuilder<TClient> : IGlobalSetupStarter<TClient>,ISetupStarter<TClient>,IVerifyStarter<TClient> where TClient: notnull
{
    public VerifierConfiguration Configuration { get; } = new ();
    //private CompareRequestValidator? _requestValidator;
    private Type? _comparerType;
    private Type? _objectCreatorType;

    private ITestObjectCreator? _objectCreator;
    private IObjectsComparer? _objectComparer;
    private IParameterMatchStrategy _matchStrategy=new PositionMatchStrategy();

    private Func<CompareRequestValidator,Task<TClient>> _clientFactory= _ =>
    {
        return Task.FromResult(Activator.CreateInstance<TClient>());
    };

    ISetupMethod ISetupStarter<TClient>.Setup(Expression<Action<TClient>> method)
    {
        return SetupImplementation(method);
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UseNameMatchingStrategy()
    {
        _matchStrategy = new NameMatchStrategy();
        return this;
    }
    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UsePositionMatchingStrategy()
    {
        _matchStrategy = new PositionMatchStrategy();
        return this;
    }

    public IGlobalSetupStarter<TClient> CheckExceptionHandling<T>() where T:Exception
    {
        Configuration.ExceptionsToCheck.Add(typeof(T));
        return this;
    }

    private ISetupMethod SetupImplementation(Expression method)
    {
        LambdaExpression lambda = (LambdaExpression)method;
        if (lambda.Body is MethodCallExpression mc)
        {
            var methodConfig = CreateMethodConfiguration(mc);
            var parameters = methodConfig.MethodInfo.GetParameters();

            for (var index = 0; index < mc.Arguments.Count; index++)
            {
                var parameterInfo = parameters[index];
                if (!methodConfig.Parameters.TryGetValue(parameterInfo, out var paramConfig))
                {
                    paramConfig = new ParameterConfiguration(parameterInfo);
                    methodConfig.Parameters.Add(parameterInfo, paramConfig);
                }

                var argument = mc.Arguments[index];
                if (argument is MethodCallExpression mce)
                {
                    if (mce.Method.DeclaringType == typeof(Behavior))
                    {
                        if (mce.Method.Name == nameof(Behavior.Generate))
                        {
                        }
                        else if (mce.Method.Name == nameof(Behavior.Transform))
                        {
                            throw new InvalidOperationException("Cannot use Transform in Setup");
                        }
                    }
                    else
                    {
                        paramConfig.SetupExpression = mce;
                    }
                }
                else
                {
                    paramConfig.SetupExpression = argument;
                }
            }


            return new SetupMethodBuilder(methodConfig);
        }

        throw new ArgumentException("This construction is not supported");
    }

    ISetupMethod ISetupStarter<TClient>.Setup<R>(Expression<Func<TClient, R>> method)
    {
        return SetupImplementation(method);
    }

    IVerifyFuncTransform IVerifyStarter<TClient>.Verify<R>(Expression<Func<TClient, R>> method)
    {
        return (IVerifyFuncTransform)VerifyImplementation(method);
    }

    
    IVerifyTransform IVerifyStarter<TClient>.Verify(Expression<Action<TClient>> method)
    {
        return VerifyImplementation(method);
    }


    private IVerifyTransform VerifyImplementation(Expression method)
    {
        LambdaExpression lambda = (LambdaExpression)method;
        if (lambda.Body is MethodCallExpression mc)
        {
            var methodConfig = CreateMethodConfiguration(mc);
            var parameters = methodConfig.MethodInfo.GetParameters();
            for (var index = 0; index < mc.Arguments.Count; index++)
            {
                var parameterInfo = parameters[index];
                if (!methodConfig.Parameters.TryGetValue(parameterInfo, out var paramConfig))
                {
                    paramConfig = new ParameterConfiguration(parameterInfo);
                    methodConfig.Parameters.Add(parameterInfo, paramConfig);
                }

                var argument = mc.Arguments[index];
                if (argument is MethodCallExpression mce)
                {
                    if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name == nameof(Behavior.Ignore))
                    {
                        paramConfig.VerifyBehavior = VerifyBehavior.Ignore;
                    }
                    else if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name == nameof(Behavior.Verify))
                    {
                        VerifyBehaviorConfiguration(mce, paramConfig);
                    }
                    if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name == nameof(Behavior.Generate))
                    {
                        paramConfig.VerifyBehavior = VerifyBehavior.GenerateValue;
                    }
                    else if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name != nameof(Behavior.Transform))
                    {

                    }
                    else
                    {
                        paramConfig.VerifyBehavior = VerifyBehavior.Transform;
                        paramConfig.VerifyExpression = mce;
                    }

                }
                else if (argument is UnaryExpression ue)
                {
                    if (ue.Operand is MethodCallExpression omc)
                    {
                        if (omc.Method.DeclaringType == typeof(Behavior) && omc.Method.Name == nameof(Behavior.Verify))
                        {
                            throw new InvalidCastException($"Wrong parameter type for {parameterInfo.Name}");
                        }
                        
                    }
                }
                else
                {
                    paramConfig.VerifyExpression = argument;
                }
            }


            return new VerifyTransformationBuilder(methodConfig);
        }

        throw new ArgumentException("This construction is not supported");
    }

    private static void VerifyBehaviorConfiguration(MethodCallExpression mce, ParameterConfiguration paramConfig)
    {
        var list = mce.Method.GetParameters();
        for (int i = 0; i < list.Length; i++)
        {
            var param = list[i];
            if (param.Name == "actionParameterName")
            {
                var actionParamNameExpression = (ConstantExpression)mce.Arguments[0];
                paramConfig.Name = (string?)actionParamNameExpression.Value;
            }
            else if (param.Name == "ignoreValue")
            {
                var ignoreValueExpression = (ConstantExpression)mce.Arguments[0];
                paramConfig.IgnoreValue = ignoreValueExpression.Value;
            }
        }
    }

    private MethodConfiguration CreateMethodConfiguration(MethodCallExpression methodExpression )
    {
        var methodInfo = methodExpression.Method;
        if (methodExpression.Method.IsGenericMethod)
        {
            methodInfo = methodExpression.Method.GetGenericMethodDefinition();
        }

        if (!Configuration.Methods.TryGetValue(methodExpression.Method, out var methodConfig))
        {
            methodConfig = new MethodConfiguration(methodInfo);
            Configuration.Methods.Add(methodInfo, methodConfig);
        }

        if (methodExpression.Method.IsGenericMethod)
        {
            methodConfig.GenericParameters = methodExpression.Method.GetGenericArguments();
            methodConfig.ReturnType = methodExpression.Method.ReturnType.GetTypeWithoutTask();
        }

        return methodConfig;
    }


    void IVerifyStarter<TClient>.Transform(Action<ParameterInfo, ParameterValue> method)
    {
        Configuration.VerifyParameterAction = method;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.SetMode(EngineMode mode)
    {
        Configuration.Mode = mode;
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UseComparer<T>()
    {
        _comparerType = typeof(T);
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UseComparer<T>(T comparer)
    {
        _objectComparer = comparer;
        return this;
    }
    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UseObjectCreator<T>() 
    {
        _objectCreatorType = typeof(T);
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.UseObjectCreator<T>(T objCreator)
    {
        _objectCreator = objCreator;
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.ConfigureVerify(Action<IVerifyStarter<TClient>> config)
    {
        config(this);
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.ConfigureSetup(Action<ISetupStarter<TClient>> config)
    {
        config(this);
        return this;
    }

    public RestVerifierEngine<TClient> Build()
    {
        return new RestVerifierEngine<TClient>(this);
    }


    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.CreateClient(Func<CompareRequestValidator,Task<TClient>> factory)
    {

        _clientFactory = factory;
        return this;
    }

    void IVerifyStarter<TClient>.ReturnTransform<T>(Func<T, object?> func)
    {
        Configuration.ReturnTransforms[typeof(T)] = func;
    }

    void IVerifyStarter<TClient>.ReturnTransform<T, R>(Func<T, R> func)
    {

    }

    void IVerifyStarter<TClient>.Transform<T>(Func<T, object?> func)
    {
        Configuration.ParameterTransforms[typeof(T)] = func;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.OnMethodExecuted(Func<ExecutionContext, Task>? func)
    {
        Configuration.MethodExecuted = func;
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.OnMethodExecuting(Func<ExecutionContext, Task>? func)
    {
        Configuration.MethodExecuting = func;
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.GetMethods(Func<Type, MethodInfo[]> method)
    {
        Configuration.GetMethodFunc = method;
        return this;
    }

    private ITestObjectCreator CreateObjectCreator()
    {
        if (_objectCreator != null)
        {
            return _objectCreator;
        }
        if (_objectCreatorType != null)
        {
            return (ITestObjectCreator)Activator.CreateInstance(_objectCreatorType);
        }

        throw new ArgumentNullException(
            "You must set ObjectCreator in your configuration. Please use UseObjectCreator method for this");
    }

    private IObjectsComparer CreateObjectsComparer()
    {
        if (_objectComparer != null)
        {
            return _objectComparer;
        }
        if (_comparerType != null)
        {
            return (IObjectsComparer)Activator.CreateInstance(_comparerType);
        }
        throw new ArgumentNullException(
            "You must set ObjectsComprarer in your configuration. Please use UseComparer method for this");
    }
    internal CompareRequestValidator CreateValidator()
    {
        var objCreator = CreateObjectCreator();
        var objTester = CreateObjectsComparer();
        var requestValidator = new CompareRequestValidator(Configuration,objTester, objCreator,_matchStrategy);
        
        return requestValidator;
    }

    
    internal Task<TClient> CreateClient(CompareRequestValidator validator)
    {
        var client = _clientFactory(validator);
        return client;
    }
    
}