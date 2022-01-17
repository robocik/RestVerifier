using System;

namespace RestVerifier
{
    public record ParameterValue(string Name,object? Value, bool Ignore = false)
    {
        public string Name { get; } = Name;
        public object? Value { get; set; } = Value;
        public object? ValueToCompare { get; set; } = Value;
        public bool Ignore { get; set; } = Ignore;
    }
}