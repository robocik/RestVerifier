using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;

//class MockRestVerifierEngine : RestVerifierEngineBase<TestClient>
//{
//    protected override Task Invoke(Func<TestClient, Task> action)
//    {
//        return Task.CompletedTask;
//    }
//}

[TestFixture]
public class RestVerifierEngineBaseTests
{
    private IGlobalSetupStarter<TestClient> _builder=null!;
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
    public async Task Skip_method()
    {
        
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Data.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Data.Generate<string>(), Data.Generate<decimal>(), Data.Generate<float>())).Skip();
            v.Setup(c => c.GetMethod3(Data.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(1,_client.Data.Count);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod1)]);
    }

    [Test]
    public async Task default_get_methods_skip_property_getters()
    {
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(3, _client.Data.Count);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod1)]);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod2)]);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod3)]);
    }

    [Test]
    public async Task specified_method_parameter_value()
    {

        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Data.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Data.Generate<string>(),12, Data.Generate<float>()));
            v.Setup(c => c.GetMethod3(Data.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual("test value",_client.Data[nameof(TestClient.GetMethod1)]["param2"]);
        Assert.AreEqual(12, _client.Data[nameof(TestClient.GetMethod2)]["param2"]);
    }
    
    
    
    [Test]
    public async Task Skip_parameter()
    {
        _builder.ConfigureVerify(v =>
        {
            v.Verify(c =>
                c.GetMethod2(Behavior.Verify<string>(), Behavior.Ignore<decimal>(), Behavior.Verify<float>()));
        }).ConfigureSetup(x=>
        {
            x.Setup(g => g.GetMethod1(Data.Generate<int>(), Data.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod3(Data.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod2)];
        Assert.AreEqual(3,data.Values.Count);
        var context=(ValidationContext)_client.RequestValidator.Context;
        var ignoredParam=context.Values.Single(x => x.Ignore);
        Assert.AreEqual("param2",ignoredParam.Name);
    }
    
    [Test]
    public async Task type_return_transformation()
    {
        bool invoked = false;
        _builder.ReturnTransform<string>(h =>
            {
                invoked = true;
                return h;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Data.Generate<int>(), Data.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Data.Generate<string>(), Data.Generate<decimal>(),Data.Generate<float>())).Skip();
            });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(invoked);
        Assert.AreEqual(1,data.Values.Count);
    }
    
    [Test]
    public async Task type_return_transformation_base_type()
    {
        bool invoked = false;
        _builder.ReturnTransform<ITestParam>(h =>
            {
                invoked = true;
                return h;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Data.Generate<int>(), Data.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Data.Generate<TestParam>())).Skip();
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(invoked);
    }
    
    [Test]
    public async Task type_return_transformation_and_method_return_transformation()
    {
        bool invokedType = false;
        bool invokedMethod = false;
        _builder.ReturnTransform<string>(h =>
            {
                invokedType = true;
                return h;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Data.Generate<int>(), Data.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Data.Generate<string>(), Data.Generate<decimal>(),Data.Generate<float>())).Skip();
            })
            .ConfigureVerify(x=>x.Verify(u=>u.GetMethod3(Behavior.Verify<TestParam>())).Returns<string,string>(k =>
            {
                invokedMethod = true;
                return k;
            }));
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(invokedMethod);
        Assert.IsFalse(invokedType);
        Assert.AreEqual(1,data.Values.Count);
    }
}