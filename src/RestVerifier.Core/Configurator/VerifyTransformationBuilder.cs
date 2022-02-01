using System;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.Configurator;


sealed class VerifyTransformationBuilder : IVerifyFuncTransform,IVerifyTransform
{
    private readonly MethodConfiguration _methodConfig;

    public VerifyTransformationBuilder(MethodConfiguration methodConfig)
    {
        _methodConfig = methodConfig;
        
    }

    

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1>(Func<P1, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1, P2>(Func<P1, P2, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1, P2, P3, P4, P5>(Func<P1, P2, P3, P4, P5, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1, P2, P3, P4, P5, P6>(Func<P1, P2, P3, P4, P5, P6, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform(Func<object?[], object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    

    IVerifyTransform IVerifyTransform.NoReturn()
    {
        _methodConfig.ShouldNotReturnValue = true;
        return this;
    }

    IVerifyTransform IVerifyTransform.SuppressExceptionHandling()
    {
        _methodConfig.SuppressExceptionHandling = true;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.SuppressCheckExceptionHandling()
    {
        _methodConfig.SuppressExceptionHandling = true;
        return this;
    }

    IVerifyTransform IVerifyTransform.Transform<P1>(Func<P1, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1, P2>(Func<P1, P2, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1, P2, P3, P4, P5>(Func<P1, P2, P3, P4, P5, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform<P1, P2, P3, P4, P5, P6>(Func<P1, P2, P3, P4, P5, P6, object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Transform(Func<object?[], object?[]> method)
    {
        _methodConfig.Transform = method;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.Returns<P>(Func<P, object?> transform)
    {
        if (_methodConfig.MethodInfo.ReturnType.GetTypeWithoutTask() != typeof(P))
        {
            throw new ArgumentException($"Returns method must have generic parameter the same as method return type, which is {_methodConfig.MethodInfo.ReturnType}");
        }
        _methodConfig.ReturnTransform = transform;
        return this;
    }

    IVerifyFuncTransform IVerifyFuncTransform.NoReturn()
    {
        _methodConfig.ShouldNotReturnValue = true;
        return this;
    }
}