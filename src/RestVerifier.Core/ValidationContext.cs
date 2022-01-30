using System;
using System.Collections.Generic;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core;


public class ValidationContext : IRemoteServiceContext
{
    public static object NotSet = new object();

    private object? returnObject= NotSet;

    private List<ParameterValue> values = new();
    private List<ParameterValue> valuesToCompare = new();
    private bool _reachEndpoint;
    public IEnumerable<ParameterValue> Values => values;

    public bool ReachEndpoint
    {
        get => _reachEndpoint;
        private set => _reachEndpoint = value;
    }

    public IEnumerable<ParameterValue> ValuesToCompare => valuesToCompare;

    public object? ReturnObject => returnObject;

    public ICollection<Exception> Exceptions { get; } = new List<Exception>();

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
        Exceptions.Clear();
        ReachEndpoint = false;
    }

    public void AddException(Exception exception)
    {
        Exceptions.Add(exception);
    }

    public void MarkReachEndpoint()
    {
        ReachEndpoint = true;
    }
}