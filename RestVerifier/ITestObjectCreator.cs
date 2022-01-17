using System;

namespace RestVerifier;

public interface ITestObjectCreator
{
    object? Create(Type type);
}