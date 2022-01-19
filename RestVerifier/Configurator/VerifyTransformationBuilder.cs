using System;
using RestVerifier.Interfaces;

namespace RestVerifier.Configurator;


public class VerifyTransformationBuilder : IVerifyFuncTransform
{
    private readonly MethodConfiguration _methodConfig;

    public VerifyTransformationBuilder(MethodConfiguration methodConfig)
    {
        _methodConfig = methodConfig;
    }

    public void Transform<P1>(Func<P1, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform<P1, P2>(Func<P1, P2, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform<P1, P2, P3, P4, P5>(Func<P1, P2, P3, P4, P5, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform<P1, P2, P3, P4, P5, P6>(Func<P1, P2, P3, P4, P5, P6, object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Transform(Func<object?[], object?[]> method)
    {
        _methodConfig.Transform = method;
    }

    public void Returns<P, R>(Func<P,R> transform)
    {
        _methodConfig.ReturnTransform = transform;
    }
}