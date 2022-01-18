using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;

[TestFixture]
public class RestVerifierEngineBase_InvalidUsage_Tests
{
    private IGlobalSetupStarter<TestClient> _builder = null!;
    TestClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _builder = RestVerifierEngineBase<TestClient>.Create();
        _builder.CreateClient((validator) =>
        {
            _client = new TestClient(validator);
            return _client;
        });

    }

    [Test]
    public void use_transform_in_setup()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _builder.ConfigureSetup(v =>
            {
                v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), Behavior.Transform<string>(h => Guid.NewGuid())));
                v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
                v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            });
        });

    }
}