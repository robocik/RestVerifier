using System;

namespace RestVerifier.Core.Interfaces;

public interface ITestObjectCreator
{
    object? Create(Type type);
}