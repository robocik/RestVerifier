using System;
using System.Reflection;
using NUnit.Framework;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Tests;

[TestFixture]
public class VerifierConfigurationBuilderTests_Global
{
    private VerifierConfigurationBuilder<TestClient> builder = new();
    private IGlobalSetupStarter<TestClient> starter = null!;

    [SetUp]
    public void Setup()
    {
        builder = new();
        starter = builder;
    }

   
    [Test]
    public void Parse_get_methods()
    {
        var oldFunc = builder.Configuration.GetMethodFunc;
        Func<Type, MethodInfo[]> func = (type) =>
        {
            return type.GetMethods();
        };
        starter.GetMethods(func);


        Assert.AreEqual(0, builder.Configuration.Methods.Count);
        Assert.IsNotNull(builder.Configuration.GetMethodFunc);
        Assert.AreNotEqual(oldFunc,builder.Configuration.GetMethodFunc);
        Assert.IsNull(builder.Configuration.VerifyParameterAction);
        Assert.AreEqual(func, builder.Configuration.GetMethodFunc);
    }
}