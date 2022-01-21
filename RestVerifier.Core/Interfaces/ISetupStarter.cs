using System;
using System.Linq.Expressions;

namespace RestVerifier.Core.Interfaces;


public interface ISetupStarter<TClient> where TClient : notnull
{
    ISetupMethod Setup<R>(Expression<Func<TClient, R>> method);

    ISetupMethod Setup(Expression<Action<TClient>> method);
}

public interface ISetupMethod
{
    void Skip();
}
