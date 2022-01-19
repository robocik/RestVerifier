using System;
using System.Linq.Expressions;
using System.Reflection;
using RestVerifier.Configurator;

namespace RestVerifier.Interfaces;

public interface IVerifyStarter<TClient>
{
    IVerifyTransform Verify<R>(Expression<Func<TClient, R>> method);

    IVerifyTransform Verify(Expression<Action<TClient>> method);

    
}



public interface ISetupStarter<TClient>
{
    ISetupMethod Setup<R>(Expression<Func<TClient, R>> method);
}

public interface ISetupMethod
{
    void Skip();
}
