using System;

namespace RestVerifier.Core.Interfaces;

public interface IVerifyTransform
{
    IVerifyTransform Transform<P1>(Func<P1, object?[]> method);

    IVerifyTransform Transform<P1, P2>(Func<P1, P2, object?[]> method);

    IVerifyTransform Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method);

    IVerifyTransform Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method);
    IVerifyTransform Transform<P1, P2, P3, P4,P5>(Func<P1, P2, P3, P4,P5, object?[]> method);
    IVerifyTransform Transform<P1, P2, P3, P4,P5,P6>(Func<P1, P2, P3, P4,P5,P6, object?[]> method);

    IVerifyTransform Transform(Func<object?[], object?[]> method);

    IVerifyTransform NoReturn();

}

public interface IVerifyFuncTransform
{
    IVerifyFuncTransform Returns<P>(Func<P,object?> transform);

    IVerifyFuncTransform Transform<P1>(Func<P1, object?[]> method);

    IVerifyFuncTransform Transform<P1, P2>(Func<P1, P2, object?[]> method);

    IVerifyFuncTransform Transform<P1, P2, P3>(Func<P1, P2, P3, object?[]> method);

    IVerifyFuncTransform Transform<P1, P2, P3, P4>(Func<P1, P2, P3, P4, object?[]> method);
    IVerifyFuncTransform Transform<P1, P2, P3, P4,P5>(Func<P1, P2, P3, P4,P5, object?[]> method);
    IVerifyFuncTransform Transform<P1, P2, P3, P4,P5,P6>(Func<P1, P2, P3, P4,P5,P6, object?[]> method);

    IVerifyFuncTransform Transform(Func<object?[], object?[]> method);

    IVerifyFuncTransform NoReturn();
}