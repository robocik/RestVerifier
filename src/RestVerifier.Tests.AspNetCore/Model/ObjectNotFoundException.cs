using System.Runtime.Serialization;

namespace RestVerifier.Tests.AspNetCore.Model;

public class ObjectNotFoundException : Exception
{
    public ObjectNotFoundException()
    {

    }
    public ObjectNotFoundException(string message) : base(message)
    {
    }

    public ObjectNotFoundException(string message, Exception innerException) : base(message, innerException)
    {
    }
    
}