using System;
using System.Linq.Expressions;
using System.Reflection;

namespace RestVerifier.Core.Interfaces;

public interface IVerifyStarter<TClient> where TClient:notnull
{
    IVerifyFuncTransform Verify<R>(Expression<Func<TClient, R>> method);

    IVerifyTransform Verify(Expression<Action<TClient>> method);

    void ReturnTransform<T>(Func<T, object?> func);

    void ReturnTransform<T,R>(Func<T, R> func);

    void Transform<T>(Func<T, object?> func);

    void Transform(Action<ParameterInfo, ParameterValue> method);
}


