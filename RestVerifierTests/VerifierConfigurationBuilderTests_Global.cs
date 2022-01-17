using System;
using System.Reflection;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;

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
    public void Parse_verify_parameter()
    {
        Action<PropertyInfo, ParameterValue> func = (paramInfo, param) =>
        {
            if (param.Value is decimal)
            {
                param.Ignore = true;
            }
        };
        starter.VerifyParameter(func);


        Assert.AreEqual(0,builder.Methods.Count);
        Assert.IsNull(builder.GetMethodFunc);
        Assert.IsNotNull(builder.VerifyParameterAction);
        Assert.AreEqual(func,builder.VerifyParameterAction);
    }

    [Test]
    public void Parse_get_methods()
    {
        Func<Type, MethodInfo[]> func = (type) =>
        {
            return type.GetMethods();
        };
        starter.GetMethods(func);


        Assert.AreEqual(0, builder.Methods.Count);
        Assert.IsNotNull(builder.GetMethodFunc);
        Assert.IsNull(builder.VerifyParameterAction);
        Assert.AreEqual(func, builder.GetMethodFunc);
    }
}