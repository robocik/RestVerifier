using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using RestVerifier.Interfaces;

namespace RestVerifier.Configurator;

public class VerifierConfiguation
{
    
    
    public Dictionary<Type, Delegate> ReturnTransforms { get; } = new();

    public Dictionary<MethodInfo, MethodConfiguration> Methods { get;}= new();

    public Action<ParameterInfo, ParameterValue>? VerifyParameterAction { get; internal set; }

    public Func<Type, MethodInfo[]> GetMethodFunc { get; internal set; }= (type) =>
    {
        return type.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public).Where(m => !m.IsSpecialName).ToArray();
    };

    public Delegate? GetReturnTransform(Type type)
    {
        if (ReturnTransforms.TryGetValue(type, out var transform))
        {
            return transform;
        }
        foreach (var configurationReturnTransform in ReturnTransforms)
        {
            if (configurationReturnTransform.Key.IsAssignableFrom(type))
            {
                return configurationReturnTransform.Value;
            }
        }

        return null;
    }
}
public class VerifierConfigurationBuilder<TClient> : IGlobalSetupStarter<TClient>,ISetupStarter<TClient>,IVerifyStarter<TClient>
{
    public VerifierConfiguation Configuation { get; } = new ();
    private CompareRequestValidator? _requestValidator;
    private Type? _comparerType;
    private Type? _objectCreatorType;
   

    private ITestObjectCreator? _objectCreator;
    private IObjectsComparer? _objectComparer;
    
    private Func<CompareRequestValidator,TClient> _clientFactory= (crv) =>
    {
        return Activator.CreateInstance<TClient>();
    };


    ISetupMethod ISetupStarter<TClient>.Setup<R>(Expression<Func<TClient, R>> method)
    {
        if (method.Body is MethodCallExpression mc)
        {
            if (!Configuation.Methods.TryGetValue(mc.Method, out var methodConfig))
            {
                methodConfig = new MethodConfiguration();
                Configuation.Methods.Add(mc.Method, methodConfig);
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

                    if (mce.Method.DeclaringType == typeof(Data) && mce.Method.Name == nameof(Data.Generate))
                    {

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

    IVerifyTransform IVerifyStarter<TClient>.Verify<R>(Expression<Func<TClient, R>> method)
    {
        return VerifyImplementation(method);
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
            if (!Configuation.Methods.TryGetValue(mc.Method, out var methodConfig))
            {
                methodConfig = new MethodConfiguration();
                Configuation.Methods.Add(mc.Method, methodConfig);
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
                    else if (mce.Method.DeclaringType == typeof(Behavior) && mce.Method.Name == nameof(Behavior.Verify))
                    {
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
    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.VerifyParameter(Action<ParameterInfo, ParameterValue> method)
    {
        Configuation.VerifyParameterAction = method;
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

    public RestVerifierEngineBase<TClient> Build()
    {
        return new RestVerifierEngineBase<TClient>(this);
    }


    public IGlobalSetupStarter<TClient> CreateClient(Func<CompareRequestValidator,TClient> factory)
    {

        _clientFactory = factory;
        return this;
    }

    public IGlobalSetupStarter<TClient> ReturnTransform<T>(Func<T, object> func)
    {
        Configuation.ReturnTransforms[typeof(T)] = func;
        return this;
    }

    IGlobalSetupStarter<TClient> IGlobalSetupStarter<TClient>.GetMethods(Func<Type, MethodInfo[]> method)
    {
        Configuation.GetMethodFunc = method;
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
        return new AutoFixtureObjectCreator();
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
        return new FluentAssertionComparer();
    }
    internal CompareRequestValidator CreateValidator()
    {
        if (_requestValidator == null)
        {
            var objCreator = CreateObjectCreator();
            var objTester = CreateObjectsComparer();
            _requestValidator = new CompareRequestValidator(Configuation,objTester, objCreator);
        }
        
        return _requestValidator;
    }


    internal TClient CreateClient()
    {
        var validator = CreateValidator();
        return _clientFactory(validator);
    }
    
}