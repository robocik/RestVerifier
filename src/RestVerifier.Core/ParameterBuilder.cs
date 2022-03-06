using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RestVerifier.Core.Configurator;

namespace RestVerifier.Core;

sealed class ParameterBuilder
{
    private readonly VerifierConfiguration _configuration;
    private readonly CompareRequestValidator _requestValidator;

    public ParameterBuilder(VerifierConfiguration configuration, CompareRequestValidator requestValidator)
    {
        _configuration = configuration;
        _requestValidator = requestValidator;
    }

    internal IList<object?> AddParameters(MethodConfiguration methodConfig, ParameterInfo[] parameters)
    {
        
        var list = new List<ParameterValue>();
        foreach (var parameterInfo in parameters)
        {
            ParameterConfiguration? paramConfig = null;
            string name=parameterInfo.Name;
            bool ignore=false;
            if (methodConfig?.Parameters.TryGetValue(parameterInfo, out paramConfig) == true)
            {
                ignore = paramConfig!.VerifyBehavior == VerifyBehavior.Ignore;
                name = paramConfig.GetName();
            }
            var paramValue = new ParameterValue(name)
            {
                Ignore = ignore
            };
            
            
            paramValue.Value = EvaluateInitialValue(paramConfig, parameterInfo);

            paramValue.ValueToCompare = EvaluateVerifyValue(paramConfig, paramValue);
            if (paramConfig!=null && Equals(paramValue.ValueToCompare,paramConfig.IgnoreValue))
            {
                paramValue.Ignore = true;
            }
            _configuration.VerifyParameterAction?.Invoke(parameterInfo, paramValue);
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
            arr = list.Where(x => !x.Ignore).ToArray();
        }
        _requestValidator.Context.AddParameters(list.ToArray());
        _requestValidator.Context.AddValues(arr);
        return list.Select(x => x.Value).ToList();
    }

    private object? EvaluateVerifyValue(ParameterConfiguration? paramConfig, ParameterValue paramValue)
    {
        var value = paramValue.Value;

        var type = paramConfig?.Parameter.ParameterType ?? value?.GetType();
        if (type != null)
        {
            var transform = _configuration.GetParameterTransform(type);
            if (transform != null)
            {
                value = (object?)transform.DynamicInvoke(value);
            }
        }

        if (paramConfig?.VerifyExpression != null)
        {
            if (paramConfig.VerifyExpression is MethodCallExpression mce)
            {
                if (mce.Method.DeclaringType == typeof(Behavior))
                {

                    if (mce.Method.Name == nameof(Behavior.Transform))
                    {
                        UnaryExpression expression = (UnaryExpression)mce.Arguments[0];
                        var exp1 = (LambdaExpression)expression.Operand;
                        value = exp1.Compile().DynamicInvoke(value);
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
            value = _requestValidator.Creator.Create(paramConfig.Parameter.ParameterType);
        }
        return value;
    }

    private object? EvaluateInitialValue(ParameterConfiguration? paramConfig, ParameterInfo parameterInfo)
    {
        object? value = null;
        if (paramConfig?.SetupExpression != null)
        {
            value = Expression.Lambda(paramConfig.SetupExpression).Compile().DynamicInvoke();
        }
        else if (value == null)
        {
            value = _requestValidator.Creator.Create(parameterInfo.ParameterType);
        }

        return value;
    }

}