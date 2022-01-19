using System;

namespace RestVerifier.Interfaces;

public interface ITestObjectCreator
{
    object? Create(Type type);
}