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



public abstract class RestVerifierEngineBase<TClient> 
{
    protected CompareRequestValidator Validator { get; private set; } = null!;
    
    public async Task TestService()
    {
        Validator = CreateValidator();
        var builder = new VerifierConfigurationBuilder<TClient>();
        Configure(builder);

        await Invoke(async client =>
        {
            try
            {
                var methods = GetMethodsToVerify(client.GetType());
                for (var index = 0; index < methods.Length; index++)
                {
                    var methodInfo = methods[index];
                    try
                    {
                        if (methodInfo.Name == "GetWebFormPdf")
                        {

                        }
                        if (builder.Methods.TryGetValue(methodInfo, out var methodConfig))
                        {
                            if (methodConfig.Skip)
                            {
                                continue;
                            }
                        }
                        ParameterInfo[] parameters = methodInfo.GetParameters();

                        Console.WriteLine("METHOD: " + methodInfo.Name+" - " + index);
                        Validator.Reset(methodConfig);

                        Validator.RegisterClientReturnType(methodInfo.ReturnType);

                        IList<object?> values = AddParameters(methodInfo,methodConfig, parameters);

                        var returnObj = await InvokeMethod(methodInfo, client, values.ToArray());
                        Validator.ValidateReturnValue(returnObj);
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e);
                        throw;
                    }
                    

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
        });

    }

    protected virtual void Configure(IVerifierConfigurator<TClient> config)
    {

    }

    

    protected virtual MethodInfo[] GetMethodsToVerify(Type type)
    {
        return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public);
    }

    private IList<object?> AddParameters(MethodInfo method,MethodConfiguration? methodConfig, ParameterInfo[] parameters)
    {
        var list = new List<ParameterValue>();
        if (method.Name == "GetInternalIndices")
        {
            
        }
        foreach (var parameterInfo in parameters)
        {

            var value = Validator.Creator.Create(parameterInfo.ParameterType);
            var paramValue = new ParameterValue(parameterInfo.Name!, value);
            if (methodConfig?.Parameters.TryGetValue(parameterInfo, out var paramConfig) == true)
            {
                if (paramConfig!.VerifyExpression is MethodCallExpression mce)
                {
                    if (mce.Method.DeclaringType == typeof(Behavior))
                    {
                        if (mce.Method.Name == nameof(Behavior.Transform))
                        {
                            UnaryExpression expression = (UnaryExpression)mce.Arguments[0];
                            var exp1 = (LambdaExpression)expression.Operand;
                            var yu = exp1.Compile().DynamicInvoke(value);
                            paramValue.ValueToCompare = yu;
                        }
                    }
                    else
                    {
                        var yu = Expression.Lambda(paramConfig.VerifyExpression).Compile().DynamicInvoke();
                        //var yu = ne.Constructor!.Invoke(ne.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray());
                        paramValue.Value = paramValue.ValueToCompare = yu;
                    }
                }
                else if (paramConfig.VerifyExpression is NewExpression ne)
                {
                    var yu = Expression.Lambda(ne).Compile().DynamicInvoke();
                    //var yu = ne.Constructor!.Invoke(ne.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray());
                    paramValue.Value = paramValue.ValueToCompare = yu;
                }
                else if (paramConfig.VerifyExpression is ConditionalExpression ce)
                {
                    var yu = Expression.Lambda(ce).Compile().DynamicInvoke();
                    //var yu = ne.Constructor!.Invoke(ne.Arguments.Select(a => ((ConstantExpression)a).Value).ToArray());
                    paramValue.Value = paramValue.ValueToCompare = yu;
                }

            }
            
            OnParameterAdding(method,paramValue);
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



    protected virtual void OnParameterAdding(MethodInfo method, ParameterValue paramValue)
    {
    }
    protected virtual CompareRequestValidator CreateValidator()
    {
        var objCreator = CreateObjectCreator();
        var objTester = CreateObjectsComparer();
        return new CompareRequestValidator(objTester, objCreator);
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
    

    protected abstract Task Invoke(Func<TClient, Task> action);

    protected virtual ITestObjectCreator CreateObjectCreator()
    {
        return new AutoFixtureObjectCreator();
    }

    protected virtual IObjectsComparer CreateObjectsComparer()
    {
        return new FluentAssertionComparer();
    }
}