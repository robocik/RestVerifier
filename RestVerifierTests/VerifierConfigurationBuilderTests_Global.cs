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
        Action<ParameterInfo, ParameterValue> func = (paramInfo, param) =>
        {
            if (param.Value is decimal)
            {
                param.Ignore = true;
            }
        };
        starter.VerifyParameter(func);


        Assert.AreEqual(0,builder.Configuation.Methods.Count);
        Assert.IsNotNull(builder.Configuation.VerifyParameterAction);
        Assert.AreEqual(func,builder.Configuation.VerifyParameterAction);
    }

    [Test]
    public void Parse_get_methods()
    {
        var oldFunc = builder.Configuation.GetMethodFunc;
        Func<Type, MethodInfo[]> func = (type) =>
        {
            return type.GetMethods();
        };
        starter.GetMethods(func);


        Assert.AreEqual(0, builder.Configuation.Methods.Count);
        Assert.IsNotNull(builder.Configuation.GetMethodFunc);
        Assert.AreNotEqual(oldFunc,builder.Configuation.GetMethodFunc);
        Assert.IsNull(builder.Configuation.VerifyParameterAction);
        Assert.AreEqual(func, builder.Configuation.GetMethodFunc);
    }
}