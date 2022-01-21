using System;
using System.Runtime.Serialization;

namespace RestVerifier.Core;

public class ValidationFailedException:Exception
{
    public ValidationFailedException()
    {
    }

    public ValidationFailedException(string message)
        : base(message)
    {
    }

    public ValidationFailedException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}