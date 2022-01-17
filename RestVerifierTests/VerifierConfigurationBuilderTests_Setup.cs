using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;

public class TestParam
{
    public string Prop1 { get; set; }

    public decimal Prop2 { get; set; }

    public float Prop3 { get; set; }
}
public class TestClient
{
    public string GetMethod1(int param1,string param2)
    {
        return param2;
    }

    public TestParam GetMethod2(string param1, decimal param2, float param3)
    {
        return new TestParam()
        {
            Prop1 = param1,
            Prop2 = param2,
            Prop3 = param3
        };
    }
}
[TestFixture]
public class VerifierConfigurationBuilderTests_Setup
{
    [Test]
    public void Parse_expressions()
    {
        var builder = new VerifierConfigurationBuilder<TestClient>();
        ISetupStarter<TestClient> starter = builder;
        starter.Setup(c => c.GetMethod1(Data.Generate<int>(), "test value"));
        starter.Setup(c => c.GetMethod2(Data.Generate<string>(), Data.Generate<decimal>(), Data.Generate<float>())).Skip();

        Assert.AreEqual(2,builder.Methods.Count);
        var method1 = builder.Methods.Single(x => x.Key.Name==nameof(TestClient.GetMethod1));
        var m1Param1 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m1Param2 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        Assert.AreEqual(2,method1.Value.Parameters.Count);
        Assert.IsNull(m1Param1.VerifyExpression);
        Assert.IsNull(m1Param2.VerifyExpression);
        Assert.IsTrue(m1Param1.SetupExpression is MethodCallExpression);
        Assert.IsTrue(m1Param2.SetupExpression is ConstantExpression);

        var method2 = builder.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        var m2Param1 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m2Param2 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        var m2Param3 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param3").Single();
        Assert.AreEqual(3, method2.Value.Parameters.Count);
        Assert.IsNull(m2Param1.VerifyExpression);
        Assert.IsNull(m2Param2.VerifyExpression);
        Assert.IsNull(m2Param3.VerifyExpression);
        Assert.IsTrue(m2Param1.SetupExpression is MethodCallExpression);
        Assert.IsTrue(m2Param2.SetupExpression is MethodCallExpression);
        Assert.IsTrue(m2Param3.SetupExpression is MethodCallExpression);
    }

    [Test]
    public void Parse_skip_method()
    {
        var builder = new VerifierConfigurationBuilder<TestClient>();
        ISetupStarter<TestClient> starter = builder;
        starter.Setup(c => c.GetMethod1(Data.Generate<int>(), "test value"));
        starter.Setup(c => c.GetMethod2(Data.Generate<string>(), Data.Generate<decimal>(), Data.Generate<float>())).Skip();
        
        var method1 = builder.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod1));
        Assert.IsFalse(method1.Value.Skip);

        var method2 = builder.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        Assert.IsTrue(method2.Value.Skip);
    }
}