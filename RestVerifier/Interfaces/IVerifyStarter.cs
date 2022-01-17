using System;
using System.Linq.Expressions;
using System.Reflection;
using RestVerifier.Configurator;

namespace RestVerifier.Interfaces;

public interface IVerifyStarter<TClient>
{
    IVerifyTransform Verify<R>(Expression<Func<TClient, R>> method);

    void Verify(Expression<Action<TClient>> method);

    
}

public interface IGlobalSetupStarter<TClient>
{
    void GetMethods(Func<Type, MethodInfo[]> method);

    void VerifyParameter(Action<PropertyInfo, ParameterValue> method);
}

public interface ISetupStarter<TClient>
{
    ISetupMethod Setup<R>(Expression<Func<TClient, R>> method);
}

public interface ISetupMethod
{
    void Skip();
}

public interface IVerifierConfigurator<TClient> : IVerifyStarter<TClient>, IGlobalSetupStarter<TClient>, ISetupStarter<TClient>
{

}