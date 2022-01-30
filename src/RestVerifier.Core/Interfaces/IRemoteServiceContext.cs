using System;
using System.Collections.Generic;

namespace RestVerifier.Core.Interfaces
{
    public interface IRemoteServiceContext
    {
        void AddParameters(params ParameterValue[] param);
        void AddReturnValue(object? value);

        void AddValues(params object?[] param);

        ICollection<Exception> Exceptions { get; }
    }
}