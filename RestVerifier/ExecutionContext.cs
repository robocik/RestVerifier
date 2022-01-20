using System;
using System.Reflection;

namespace RestVerifier;

public enum ExecutionResult
{
    Running,
    Success,
    Error
}
public class ExecutionContext
{

    public ExecutionContext(MethodInfo method,MethodInfo[] allMethods)
    {
        AllMethods = allMethods;
        Method = method;
    }

    public ExecutionResult Result { get; internal set; }

    public Exception? Exception { get; internal set; }

    public MethodInfo Method { get;  }

    public bool Abort { get; set; }

    public MethodInfo[] AllMethods { get; }
}