using System.Collections.Generic;

namespace RestVerifier.Core;


public class ValidationContext : IRemoteServiceContext
{
    public static object NotSet = new object();

    private object? returnObject= NotSet;

    private List<ParameterValue> values = new();
    private List<ParameterValue> valuesToCompare = new();
    public IEnumerable<ParameterValue> Values => values;

    public IEnumerable<ParameterValue> ValuesToCompare => valuesToCompare;

    public object? ReturnObject => returnObject;

    public void AddParameters(params ParameterValue[] param)
    {
        values.Clear();
        values.AddRange(param);
    }

    public void AddValues(params object?[] param)
    {
        valuesToCompare.Clear();
        foreach (var obj in param)
        {
            if (obj is ParameterValue pv)
            {
                valuesToCompare.Add(pv);
            }
            else
            {
                valuesToCompare.Add(new ParameterValue(null)
                {
                    ValueToCompare = obj
                });
            }
        }
        
    }

    public void AddReturnValue(object? value)
    {
        returnObject = value;
    }

    public void Reset()
    {
        valuesToCompare.Clear();
        values.Clear();
        returnObject = NotSet;
    }
}