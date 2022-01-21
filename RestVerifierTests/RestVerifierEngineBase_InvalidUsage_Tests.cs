using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RestVerifier.Core;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Tests;

[TestFixture]
public class RestVerifierEngineBase_InvalidUsage_Tests
{
    private IGlobalSetupStarter<TestClient> _builder = null!;
    TestClient _client = null!;

    [SetUp]
    public void Setup()
    {
        _builder = RestVerifierEngine<TestClient>.Create();
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

    [Test]
    public void generic_method_without_configuration()
    {
        Assert.ThrowsAsync<ArgumentNullException>(async () =>
        {
            _builder.SetMode(EngineMode.Loose);
            _builder.ConfigureSetup(v =>
            {
                v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>()));
                v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
                v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            });

            var engine = _builder.Build();
            await engine.TestService();
            
        });

    }
}