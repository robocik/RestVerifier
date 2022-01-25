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

    void IVerifyTransform.Transform<P1, P2>(Func<P1, P2, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform<P1, P2, P3, P4, P5>(Func<P1, P2, P3, P4, P5, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform<P1, P2, P3, P4, P5, P6>(Func<P1, P2, P3, P4, P5, P6, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform(Func<object?[], object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    void IVerifyTransform.Transform<P1>(Func<P1, object?[]> method)
    {
        _methodConfig.Transform = method;
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
        _methodConfig.ReturnTransform = transform;
        return this;
    }

}