using System;
using System.Reflection;

namespace RestVerifier;


public class VerifierExecutionException : Exception
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

    public VerifierExecutionException(MethodInfo method,string message, Exception innerException)
        : base(message, innerException)
    {
        Method = method;
    }

    public VerifierExecutionException(MethodInfo method,  Exception innerException)
        : base($"Execution of {method.Name} failed", innerException)
    {
        Method = method;
    }
}