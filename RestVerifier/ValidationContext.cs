using System.Collections.Generic;

namespace RestVerifier;


class ValidationContext : IRemoteServiceContext
{
    private object? returnObject;

    private List<ParameterValue> values = new();
    private List<object?> valuesToCompare = new();
    public IEnumerable<ParameterValue> Values => values;

    public IEnumerable<object?> ValuesToCompare => valuesToCompare;

    public object? ReturnObject => returnObject;

    public void AddParameters(params ParameterValue[] param)
    {
        values.Clear();
        values.AddRange(param);
    }

    public void AddValues(params object?[] param)
    {
        valuesToCompare.Clear();
        valuesToCompare.AddRange(param);
    }

    public void AddReturnValue(object? value)
    {
        returnObject = value;
    }

    public void Reset()
    {
        valuesToCompare.Clear();
        values.Clear();
        returnObject = null;
    }
}