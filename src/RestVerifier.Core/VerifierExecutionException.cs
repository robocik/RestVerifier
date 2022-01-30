using System;
using System.Collections.Generic;
using System.Reflection;

namespace RestVerifier.Core;


public class VerifierExecutionException : AggregateException
{
    public MethodInfo Method { get; }
    public VerifierExecutionException(MethodInfo method)
    {
        Method = method;
    }

    public VerifierExecutionException(MethodInfo method,string message)
        : base(message)
    {
        Method = method;
    }

    public VerifierExecutionException(MethodInfo method,string message, params Exception[] innerExceptions)
        : base(message, innerExceptions)
    {
        Method = method;
    }

    public VerifierExecutionException(MethodInfo method, string message, IEnumerable<Exception> innerExceptions)
        : base(message, innerExceptions)
    {
        Method = method;
    }

    public VerifierExecutionException(MethodInfo method, params Exception[] innerExceptions)
        : base($"Execution of {method.Name} failed", innerExceptions)
    {
        Method = method;
    }
    public VerifierExecutionException(MethodInfo method, IEnumerable<Exception> innerExceptions)
        : base($"Execution of {method.Name} failed", innerExceptions)
    {
        Method = method;
    }
}