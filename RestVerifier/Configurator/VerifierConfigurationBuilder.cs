using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using RestVerifier.Interfaces;

namespace RestVerifier.Configurator;

public class VerifierConfigurationBuilder<TClient> : IVerifierConfigurator<TClient>
{
    private Dictionary<MethodInfo, MethodConfiguration> _methods = new();
    private Action<PropertyInfo, ParameterValue>? _verifyParameter;
    private Func<Type, MethodInfo[]>? _getMethod;

    public Dictionary<MethodInfo, MethodConfiguration> Methods => _methods;

    public Action<PropertyInfo, ParameterValue>? VerifyParameterAction => _verifyParameter;

    public Func<Type, MethodInfo[]>? GetMethodFunc => _getMethod;


    ISetupMethod ISetupStarter<TClient>.Setup<R>(Expression<Func<TClient, R>> method)
    {
        if (method.Body is MethodCallExpression mc)
        {
            if (!Methods.TryGetValue(mc.Method, out var methodConfig))
            {
                methodConfig = new MethodConfiguration();
                Methods.Add(mc.Method, methodConfig);
            }


            for (var index = 0; index < mc.Arguments.Count; index++)
            {
                var parameterInfo = mc.Method.GetParameters()[index];
                if (!methodConfig.Parameters.TryGetValue(parameterInfo, out var paramConfig))
                {
                    paramConfig = new ParameterConfiguration(parameterInfo);
                    methodConfig.Parameters.Add(parameterInfo, paramConfig);
                }
                var argument = mc.Arguments[index];
                if (argument is MethodCallExpression mce)
                {

                    if (mce.Method.DeclaringType == typeof(Data) && mce.Method.Name != nameof(Data.Generate))
                    {

                    }
                    else
                    {
                        paramConfig.SetupExpression = mc;
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

    IVerifyTransform IVerifyStarter<TClient>.Verify<R>(Expression<Func<TClient, R>> method)
    {
        if (method.Body is MethodCallExpression mc)
        {
            if (!Methods.TryGetValue(mc.Method, out var methodConfig))
            {
                methodConfig = new MethodConfiguration();
                Methods.Add(mc.Method, methodConfig);
            }
            for (var index = 0; index < mc.Arguments.Count; index++)
            {
                var parameterInfo = mc.Method.GetParameters()[index];
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
                        paramConfig.Ignore = true;
                    }
                    else if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name != nameof(Behavior.Transform))
                    {

                    }
                    else
                    {
                        paramConfig.VerifyExpression = mce;
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

    void IVerifyStarter<TClient>.Verify(Expression<Action<TClient>> method)
    {
    }


    void IGlobalSetupStarter<TClient>.VerifyParameter(Action< PropertyInfo, ParameterValue> method)
    {
        _verifyParameter = method;
    }

    void IGlobalSetupStarter<TClient>.GetMethods(Func<Type, MethodInfo[]> method)
    {
        _getMethod = method;
    }
}