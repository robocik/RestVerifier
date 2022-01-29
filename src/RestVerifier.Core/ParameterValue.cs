namespace RestVerifier.Core
{
    public record ParameterValue(string? Name)
    {
        public string? Name { get; } = Name;
        public object? Value { get; set; } 
        public object? ValueToCompare { get; set; }
        public bool Ignore { get; set; } 
    }
}