﻿using System.Collections.Generic;
using RestVerifier.Core;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Tests;

public interface ITestParam
{
    string Prop1 { get; set; }
}
public class TestParam:ITestParam
{
    public string Prop1 { get; set; }

    public decimal Prop2 { get; set; }

    public float Prop3 { get; set; }
}
class TestClient
{
    public CompareRequestValidator RequestValidator { get; }

    public ValidationContext Context => (ValidationContext)RequestValidator.Context;
    private Dictionary<string, Dictionary<string, object>> data = new();
    public TestClient(CompareRequestValidator requestValidator)
    {
        RequestValidator = requestValidator;
    }

    public Dictionary<string, Dictionary<string, object>> Data => data;

    public string GetMethod1(int param1, string param2)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 },
            { nameof(param2), param2 }
        };
        Data.Add(nameof(GetMethod1), dict);
        return (string)RequestValidator.AddReturnType(typeof(string));
    }

    public TestParam GetMethod2(string param1, decimal param2, float param3)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 },
            { nameof(param2), param2 },
            { nameof(param3), param3 }
        };
        Data.Add(nameof(GetMethod2), dict);
        return (TestParam)RequestValidator.AddReturnType(typeof(TestParam));
    }
    
    public string GetMethod3(TestParam param1)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 }
        };
        Data.Add(nameof(GetMethod3), dict);
        var res= (TestParam)RequestValidator.AddReturnType(typeof(TestParam));
        return res.Prop1;
    }



    public void GetMethod4(TestParam param1)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 }
        };
        Data.Add(nameof(GetMethod4), dict);
    }

    public T2 GetMethod5<T1,T2>(TestParam param1,T1 param2)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 },
            { nameof(param2), param2 }
        };
        Data.Add(nameof(GetMethod5), dict);
        return (T2)RequestValidator.AddReturnType(typeof(T2));
    }

    public int GetMethod6(bool? isTest)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(isTest), isTest }
        };
        Data.Add(nameof(GetMethod5), dict);
        return (int)RequestValidator.AddReturnType(typeof(int));
    }

    public void UpdateMethod1(bool? isTest)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(isTest), isTest }
        };
        Data.Add(nameof(UpdateMethod1), dict);
        RequestValidator.AddReturnType(typeof(int));
    }

    public ITestParam GetMethod7(TestParam param1)
    {
        var dict = new Dictionary<string, object>
        {
            { nameof(param1), param1 }
        };
        Data.Add(nameof(GetMethod7), dict);
        var res = (TestParam)RequestValidator.AddReturnType(typeof(string));
        return res;
    }
    
}
