using System.Collections.Generic;
using RestVerifier;

namespace RestVerifierTests;

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
        return (string)RequestValidator.AddReturnType(typeof(TestParam));
    }
}
