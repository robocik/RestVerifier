using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Core.Interfaces;

public interface IVerifyStarter<TClient> where TClient:notnull
{
    IVerifyFuncTransform Verify<R>(Expression<Func<TClient, R>> method);

    IVerifyTransform Verify(Expression<Action<TClient>> method);

    void ReturnTransform<T>(Func<T, object?> func);

    void ParameterTransform<T>(Func<T, object?> func);

    void VerifyParameter(Action<ParameterInfo, ParameterValue> method);
}



public interface ISetupStarter<TClient> where TClient : notnull
{
    ISetupMethod Setup<R>(Expression<Func<TClient, R>> method);

    ISetupMethod Setup(Expression<Action<TClient>> method);
}

public interface ISetupMethod
{
    void Skip();
}
