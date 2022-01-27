using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using RestVerifier.Core;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Tests;

[TestFixture]
public class VerifierConfigurationBuilderTests_Verify
{
    private VerifierConfigurationBuilder<TestClient>  builder = new ();
    private IVerifyStarter<TestClient> starter=null!;

    [SetUp]
    public void Setup()
    {
        builder = new();
        starter = builder;
    }

    [Test]
    public void Parse_expressions()
    {
        starter.Verify(c => c.GetMethod1(Behavior.Verify<int>(), Behavior.Verify<string>()));
        starter.Verify(c => c.GetMethod2(Behavior.Verify<string>(), Behavior.Verify<decimal>(), Behavior.Verify<float>()));

        Assert.AreEqual(2, builder.Configuration.Methods.Count);
        var method1 = builder.Configuration.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod1));
        var m1Param1 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m1Param2 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        Assert.AreEqual(2, method1.Value.Parameters.Count);
        Assert.IsNull(m1Param1.SetupExpression);
        Assert.IsNull(m1Param2.SetupExpression);
        Assert.IsNull(m1Param1.VerifyExpression);
        Assert.IsNull(m1Param2.VerifyExpression);

        var method2 = builder.Configuration.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        var m2Param1 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m2Param2 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        var m2Param3 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param3").Single();
        Assert.AreEqual(3, method2.Value.Parameters.Count);
        Assert.IsNull(m2Param1.SetupExpression);
        Assert.IsNull(m2Param2.SetupExpression);
        Assert.IsNull(m2Param3.SetupExpression);
        Assert.IsNull(m2Param1.VerifyExpression);
        Assert.IsNull(m2Param2.VerifyExpression);
        Assert.IsNull(m2Param3.VerifyExpression);
    }


    [Test]
    public void Parse_verify_parameter()
    {
        Action<ParameterInfo, ParameterValue> func = (paramInfo, param) =>
        {
            if (param.Value is decimal)
            {
                param.Ignore = true;
            }
        };
        starter.Transform(func);


        Assert.AreEqual(0, builder.Configuration.Methods.Count);
        Assert.IsNotNull(builder.Configuration.VerifyParameterAction);
        Assert.AreEqual(func, builder.Configuration.VerifyParameterAction);
    }

    [Test]
    public void Parse_ignore_parameter()
    {
        starter.Verify(c => c.GetMethod1(Behavior.Ignore<int>(), Behavior.Verify<string>()));
        starter.Verify(c => c.GetMethod2(Behavior.Verify<string>(), Behavior.Ignore<decimal>(), Behavior.Verify<float>()));


        var method1 = builder.Configuration.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod1));
        var m1Param1 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m1Param2 = method1.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        Assert.IsTrue(m1Param1.Ignore);
        Assert.IsFalse(m1Param2.Ignore);

        var method2 = builder.Configuration.Methods.Single(x => x.Key.Name == nameof(TestClient.GetMethod2));
        var m2Param1 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param1").Single();
        var m2Param2 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param2").Single();
        var m2Param3 = method2.Value.Parameters.Values.Where(x => x.Parameter.Name == "param3").Single();
        Assert.IsFalse(m2Param1.Ignore);
        Assert.IsTrue(m2Param2.Ignore);
        Assert.IsFalse(m2Param3.Ignore);
    }


    
}