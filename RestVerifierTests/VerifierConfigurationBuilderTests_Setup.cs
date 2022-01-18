using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;



[TestFixture]
public class VerifierConfigurationBuilderTests_Setup
{
    [Test]
    public void Parse_expressions()
    {
        var builder = new VerifierConfigurationBuilder<TestClient>();
        ISetupStarter<TestClient> starter = builder;
        starter.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
        starter.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();

        Assert.AreEqual(2,builder.Configuation.Methods.Count);
        var method1 = builder.Configuation.Methods.Single(x => x.Key.Name==nameof(TestClient.GetMethod1));
        var m1Param1 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m1Param2 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        Assert.AreEqual(2,method1.Value.Parameters.Count);
        Assert.IsNull(m1Param1.VerifyExpression);
        Assert.IsNull(m1Param2.VerifyExpression);
        Assert.IsNull(m1Param1.SetupExpression );
        Assert.IsTrue(m1Param2.SetupExpression is ConstantExpression);

        var method2 = builder.Configuation.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        var m2Param1 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m2Param2 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        var m2Param3 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param3").Single();
        Assert.AreEqual(3, method2.Value.Parameters.Count);
        Assert.IsNull(m2Param1.VerifyExpression);
        Assert.IsNull(m2Param2.VerifyExpression);
        Assert.IsNull(m2Param3.VerifyExpression);
        Assert.IsNull(m2Param1.SetupExpression);
        Assert.IsNull(m2Param2.SetupExpression);
        Assert.IsNull(m2Param3.SetupExpression);
    }

    [Test]
    public void Parse_skip_method()
    {
        var builder = new VerifierConfigurationBuilder<TestClient>();
        ISetupStarter<TestClient> starter = builder;
        starter.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
        starter.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
        
        var method1 = builder.Configuation.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod1));
        Assert.IsFalse(method1.Value.Skip);

        var method2 = builder.Configuation.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        Assert.IsTrue(method2.Value.Skip);
    }
}