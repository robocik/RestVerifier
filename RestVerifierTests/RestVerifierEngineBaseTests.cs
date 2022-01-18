using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using NUnit.Framework;
using RestVerifier;
using RestVerifier.Configurator;
using RestVerifier.Interfaces;

namespace RestVerifierTests;

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
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
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
    public async Task get_methods()
    {

        _builder.GetMethods(type => type.GetMethods().Where(x => x.Name == "GetMethod2").ToArray());
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod2)]);
        Assert.AreEqual(1, _client.Data.Count);
    }



    [Test]
    public async Task Skip_parameter()
    {
        _builder.ConfigureVerify(v =>
        {
            v.Verify(c => c.GetMethod2(Behavior.Verify<string>(), Behavior.Ignore<decimal>(), Behavior.Verify<float>()));
        }).ConfigureSetup(x=>
        {
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod2)];
        Assert.AreEqual(3,data.Values.Count);
        
        var ignoredParam= _client.Context.Values.Single(x => x.Ignore);
        Assert.AreEqual("param2",ignoredParam.Name);
    }

    [Test]
    public async Task specify_simple_parameter_1()
    {

        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), 12, Behavior.Generate<float>()));
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual("test value", _client.Data[nameof(TestClient.GetMethod1)]["param2"]);
        Assert.AreEqual(12, _client.Data[nameof(TestClient.GetMethod2)]["param2"]);
    }



    [Test]
    public async Task specify_simple_parameter_2()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(c => c.GetMethod2("Specific value", Behavior.Generate<decimal>(), Behavior.Generate<float>()));
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod2)];
        Assert.IsTrue(data.Values.Any(x => x.ToString()=="Specific value"));
        var specifiedParam = _client.Context.Values.Single(x => x.Name=="param1");
        Assert.AreEqual("Specific value", specifiedParam.Value);
        Assert.AreEqual("Specific value", specifiedParam.ValueToCompare);
    }

    [Test]
    public async Task specify_class_parameter()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(c => c.GetMethod2("Specific value", Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod3(new TestParam()
            {
                Prop1="Test1"
            }));
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(data.Values.Any(x => ((TestParam)x).Prop1 == "Test1"));
        var specifiedParam = _client.Context.Values.Single(x => x.Name == "param1");
        var value = (TestParam)specifiedParam.Value!;
        var valueToCompare = (TestParam)specifiedParam.ValueToCompare!;
        Assert.AreEqual("Test1", value.Prop1);
        Assert.AreEqual("Test1", valueToCompare.Prop1);
    }

    [Test]
    public async Task specify_simple_parameter_for_verify()
    {

        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), 12, Behavior.Generate<float>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
        });
        _builder.ConfigureVerify(v =>
        {
            v.Verify(c => c.GetMethod1(Behavior.Verify<int>(), "verify value"));
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual("test value", _client.Data[nameof(TestClient.GetMethod1)]["param2"]);

        var specifiedParam = _client.Context.Values.Single(x => x.Name == "param2");
        Assert.AreEqual("test value", specifiedParam.Value);
        Assert.AreEqual("verify value", specifiedParam.ValueToCompare);
    }

    [Test]
    public async Task specify_class_parameter_for_verify()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(c => c.GetMethod2("Specific value", Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod3(new TestParam()
            {
                Prop1 = "Test1"
            }));
        });
        _builder.ConfigureVerify(v =>
        {
            v.Verify(c => c.GetMethod3(new TestParam()
            {
                Prop1 = "Verify test"
            }));
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(data.Values.Any(x => ((TestParam)x).Prop1 == "Test1"));
        var specifiedParam = _client.Context.Values.Single(x => x.Name == "param1");
        var value = (TestParam)specifiedParam.Value!;
        var valueToCompare = (TestParam)specifiedParam.ValueToCompare!;
        Assert.AreEqual("Test1", value.Prop1);
        Assert.AreEqual("Verify test", valueToCompare.Prop1);
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
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(),Behavior.Generate<float>())).Skip();
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
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
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
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(),Behavior.Generate<float>())).Skip();
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

    [Test]
    public async Task method_transform()
    {
        bool invoked = false;
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v=>v.GetMethod2(Behavior.Verify<string>(),Behavior.Verify<decimal>(),Behavior.Verify<float>())).Transform<string,decimal,float>(
                    (p1, p2, p3) =>
                    {
                        invoked = true;
                        return new object[]{ "test" };
                    });
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(invoked);
        Assert.AreEqual("test", _client.Context.ValuesToCompare.Single());
    }

    [Test]
    public async Task parameter_generate_value_for_verify()
    {
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                x.Setup(v => v.GetMethod2(Behavior.Generate<string>(), 500, Behavior.Generate<float>()));
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod2(Behavior.Verify<string>(), Behavior.Generate<decimal>(), Behavior.Verify<float>()));
            });
        var engine = _builder.Build();
        await engine.TestService();

        var param2 = _client.Context.Values.Single(x => x.Name == "param2");
        Assert.AreEqual(500,param2.Value);
        Assert.AreNotEqual(500, param2.ValueToCompare);
    }

    [Test]
    public async Task parameter_transform()
    {
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod2(Behavior.Verify<string>(), Behavior.Transform<decimal>(b => "test1"), Behavior.Verify<float>()));
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue( _client.Context.Values.Any(x=>x.ValueToCompare!.ToString()=="test1"));
    }

    [Test]
    public async Task method_and_parameter_transform()
    {
        bool invoked = false;
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod2(Behavior.Verify<string>(), Behavior.Transform<decimal>(b => "test1"), Behavior.Verify<float>())).Transform<string, string, float>(
                    (p1, p2, p3) =>
                    {
                        invoked = true;
                        return new object[] {p2+"test" };
                    });
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(invoked);
        Assert.AreEqual("test1test", _client.Context.ValuesToCompare.Single());
    }

    [Test]
    public async Task global_verify_parameter()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
        });
        _builder.VerifyParameter((x, v) =>
        {
            if (x.ParameterType == typeof(TestParam))
            {
                v.Ignore = true;
            }
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.AreEqual(1, data.Values.Count);

        var ignoredParam = _client.Context.Values.Single(x => x.Ignore);
        Assert.AreEqual("param1", ignoredParam.Name);
    }

    [Test]
    public async Task type_parameter_transformation()
    {
        bool invoked = false;
        _builder.ParameterTransform<TestParam>(h =>
            {
                invoked = true;

                var test = new TestParam();
                test.Prop1 = "Test";
                return test;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(invoked);
        var val = (TestParam)data.Values.Single();
        Assert.AreNotEqual("Test", val.Prop1);
        val= (TestParam)_client.Context.Values.Single().Value!;
        var valToCompare = (TestParam)_client.Context.Values.Single().ValueToCompare!;
        Assert.AreNotEqual("Test",val.Prop1);
        Assert.AreEqual("Test", valToCompare.Prop1);
    }

    [Test]
    public async Task type_parameter_transformation_and_param_transformation()
    {
        bool invoked = false;
        _builder.ParameterTransform<TestParam>(h =>
            {
                invoked = true;

                var test = new TestParam();
                test.Prop1 = "Test";
                return test;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            });
        _builder.ConfigureVerify(c =>
        {
            c.Verify(b => b.GetMethod3(Behavior.Transform<TestParam>(g => new TestParam()
            {
                Prop1 = "Local"
            })));
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(invoked);
        var val = (TestParam)data.Values.Single();
        Assert.AreNotEqual("Local", val.Prop1);
        val = (TestParam)_client.Context.Values.Single().Value!;
        var valToCompare = (TestParam)_client.Context.Values.Single().ValueToCompare!;
        Assert.AreNotEqual("Local", val.Prop1);
        Assert.AreEqual("Local", valToCompare.Prop1);
    }
}