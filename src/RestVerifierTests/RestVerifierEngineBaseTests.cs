using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using AutoFixture.Kernel;
using NUnit.Framework;
using NUnit.Framework.Internal;
using RestVerifier.AutoFixture;
using RestVerifier.Core;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Tests;

[TestFixture]
public class RestVerifierEngineBaseTests
{
    private IGlobalSetupStarter<TestClient> _builder=null!;
    TestClient _client = null!;

    [SetUp]
    public void Setup()
    {
        
        _builder = Engine.CreateDefaultBuilder<TestClient>();
        _builder.SetMode(EngineMode.Strict);
        _builder.CreateClient((validator) =>
        {
            _client = new TestClient(validator);
            return Task.FromResult(_client);
        });
        var creator = new AutoFixtureObjectCreator();
        creator.Fixture.Customizations.Add(new TypeRelay(typeof(ITestParam), typeof(TestParam)));
        _builder.UseObjectCreator(creator);
    }

    [Test]
    public async Task Skip_method()
    {

        _builder.SetMode(EngineMode.Loose);
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            v.Setup(c => c.GetMethod5<string,int>(Behavior.Generate<TestParam>(),Behavior.Generate<string>())).Skip();
            v.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.GetMethod7(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2,_client.Data.Count);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod1)]);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod4)]);
    }

    [Test]
    public async Task Skip_void_method()
    {

        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), "test value"));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            v.Setup(c=>c.GetMethod4(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(1, _client.Data.Count);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod1)]);
    }

    [Test]
    public async Task default_get_methods_skip_property_getters()
    {
        _builder.SetMode(EngineMode.Loose);
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
            v.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            v.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.GetMethod7(Behavior.Generate<TestParam>())).Skip();
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(3, _client.Data.Count);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod1)]);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod2)]);
        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod4)]);
    }

        [Test]
    public async Task get_methods()
    {
        _builder.SetMode(EngineMode.Loose);
        _builder.GetMethods(type => type.GetMethods().Where(x => x.Name == nameof(TestClient.GetMethod2)).ToArray());
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsNotNull(_client.Data[nameof(TestClient.GetMethod2)]);
        Assert.AreEqual(1, _client.Data.Count);
        Assert.AreEqual(nameof(TestClient.GetMethod2), engine.GetMethods().Single().Name);
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
            x.Setup(c => c.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
            x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
            x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            x.Setup(c => c.GetMethod4(new TestParam()
            {
                Prop1="Test1"
            }));
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod4)];
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
            v.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
            x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            x.Setup(c => c.GetMethod4(new TestParam()
            {
                Prop1 = "Test1"
            }));
        });
        _builder.ConfigureVerify(v =>
        {
            v.Verify(c => c.GetMethod4(new TestParam()
            {
                Prop1 = "Verify test"
            }));
        });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod4)];
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
        _builder.ConfigureVerify(x =>
            {
                x.ReturnTransform<string>(h =>
                {
                    invoked = true;
                    return new TestParam() { Prop1 = h };
                });
            })
            .ConfigureSetup(x =>
            {
                x.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>()));
            });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod3)];
        Assert.IsTrue(invoked);
        Assert.AreEqual(1,data.Values.Count);
    }

    [Test]
    public void different_return_types_but_no_returns_transformation()
    {
        _builder.ConfigureSetup(x =>
            {
                x.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>()));
            });
        var engine = _builder.Build();
        var ex=Assert.ThrowsAsync<VerifierExecutionException>(()=> engine.TestService());
        Assert.IsTrue(ex.InnerException is InvalidCastException);
    }

    [Test]
    public async Task type_return_transformation_base_type()
    {
        bool invoked = false;
        _builder.ConfigureVerify(x =>
        {
            x.ReturnTransform<ITestParam>(h =>
            {
                invoked = true;
                return h;
            });
            x.Verify(c => c.GetMethod7(Behavior.Verify<TestParam>()));
        });
        _builder.ConfigureSetup(v =>
            {
                v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
                v.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
                v.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                v.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
            })
            .SetMode(EngineMode.Loose);
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(invoked);
    }
    
    [Test]
    public async Task type_return_transformation_and_method_return_transformation()
    {
        bool invokedType = false;
        bool invokedMethod = false;
        _builder
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(),Behavior.Generate<float>())).Skip();
            })
            .ConfigureVerify(x=>
            {
                x.ReturnTransform<string>(h =>
                {
                    invokedType = true;
                    return new TestParam() { Prop1 =h };
                });
                x.Verify(u => u.GetMethod3(Behavior.Verify<TestParam>())).Returns<string>(k =>
                {
                    invokedMethod = true;
                    return new TestParam() { Prop1 = k };
                });
            });
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
                x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
        
        var param1 = _client.Context.ValuesToCompare.Single();
        Assert.AreEqual("test", param1.ValueToCompare);
        Assert.IsNull(param1.Name);
        Assert.IsNull(param1.Value);

    }

    [Test]
    public async Task method_transform_array_method()
    {
        bool invoked = false;
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod2(Behavior.Verify<string>(), Behavior.Verify<decimal>(), Behavior.Verify<float>())).Transform(
                    (arr) =>
                    {
                        Assert.AreEqual(3,arr.Length);
                        Assert.IsTrue(arr[0] is string);
                        Assert.IsTrue(arr[1] is decimal);
                        Assert.IsTrue(arr[2] is float);
                        invoked = true;
                        return new object[] { "test" };
                    });
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(invoked);
        var param1 = _client.Context.ValuesToCompare.Single();
        Assert.AreEqual("test", param1.ValueToCompare);
        Assert.IsNull(param1.Name);
        Assert.IsNull(param1.Value);
    }

    [Test]
    public async Task parameter_generate_value_for_verify()
    {
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                x.Setup(v => v.GetMethod2(Behavior.Generate<string>(), 500, Behavior.Generate<float>()));
                x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
        _builder
            .GetMethods(m=>m.GetMethods().Where(h=>h.Name==nameof(TestClient.GetMethod2)).ToArray())
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod2(Behavior.Verify<string>(), Behavior.Transform<decimal>(b => "test1"), Behavior.Verify<float>()));
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue( _client.Context.Values.Any(x=>x.ValueToCompare!.ToString()=="test1"));
    }

    [Test]
    public async Task parameter_transform_void_method()
    {
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            })
            .ConfigureVerify(c =>
            {
                c.Verify(v => v.GetMethod4( Behavior.Transform<TestParam>(b => new TestParam(){Prop1 = "test1"})));
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(_client.Context.Values.Any(x => ((TestParam)x.ValueToCompare!).Prop1 == "test1"));
    }

    [Test]
    public async Task method_and_parameter_transform()
    {
        bool invoked = false;
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
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
        var param1=_client.Context.ValuesToCompare.Single();
        Assert.AreEqual("test1test", param1.ValueToCompare);
        Assert.IsNull(param1.Name);
        Assert.IsNull(param1.Value);
    }

    [Test]
    public async Task global_verify_parameter()
    {
        _builder.SetMode(EngineMode.Loose);
        _builder.ConfigureSetup(x =>
        {
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
            x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
            x.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
            x.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
            x.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
            x.Setup(c => c.GetMethod7(Behavior.Generate<TestParam>())).Skip();
        });
        _builder.ConfigureVerify(x =>
        {
            x.Verify(g => g.GetMethod3(Behavior.Verify<TestParam>())).Returns<string>(h=>new TestParam(){Prop1 = h});
            x.Transform((x, v) =>
            {
                if (x.ParameterType == typeof(TestParam))
                {
                    v.Ignore = true;
                }
            });
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
        _builder.ConfigureVerify(x =>
            {
                x.Transform<TestParam>(h =>
                {
                    invoked = true;

                    var test = new TestParam();
                    test.Prop1 = "Test";
                    return test;
                });
            })
            .ConfigureSetup(x =>
            {
                x.Setup(c => c.GetMethod4(Behavior.Generate<TestParam>()));
                
            });
        var engine = _builder.Build();
        await engine.TestService();

        var data = _client.Data[nameof(TestClient.GetMethod4)];
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
        _builder.ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(g => g.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>())).Skip();
                x.Setup(g => g.GetMethod4(Behavior.Generate<TestParam>())).Skip();
            });
        _builder.ConfigureVerify(c =>
        {
            c.Transform<TestParam>(h =>
            {
                invoked = true;

                var test = new TestParam();
                test.Prop1 = "Test";
                return test;
            });
            c.Verify(b => b.GetMethod3(Behavior.Transform<TestParam>(g => new TestParam()
            {
                Prop1 = "Local"
            }))).Returns<string>(h=>new TestParam(){Prop1 = h});
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

    [Test]
    public async Task on_method_executed_callback()
    {
        _builder.SetMode(EngineMode.Loose);
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
            v.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
            v.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
            v.Setup(c => c.GetMethod7(Behavior.Generate<TestParam>())).Skip();
        });
        var list = new List<MethodInfo>();
        _builder.OnMethodExecuted(c =>
            {
                list.Add(c.Method);
                return Task.CompletedTask;
            })
            .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2,list.Count);
        Assert.IsFalse(list.Any(x=>x.Name==nameof(TestClient.GetMethod1)));
    }

    [Test]
    public async Task on_method_executed_callback_abort_if_success()
    {
        _builder.SetMode(EngineMode.Strict);
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
            v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>()));
            v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>()));
        });
        var list = new List<MethodInfo>();
        _builder.OnMethodExecuted(c =>
            {
                list.Add(c.Method);
                if (c.Method.Name == nameof(TestClient.GetMethod2))
                {
                    c.Abort = true;
                }

                return Task.CompletedTask;
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2, list.Count);
        Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod3)));
    }

    [Test]
    public void on_method_executed_callback_abort_if_exception()
    {
        var list = new List<MethodInfo>();
        _builder.SetMode(EngineMode.Strict);
        _builder.ConfigureSetup(v =>
        {
            v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();

        });
        _builder.ConfigureVerify(c =>
        {
            c.Verify(k => k.GetMethod2(Behavior.Transform<string>(h => "wrong"),Behavior.Verify<decimal>(), Behavior.Verify<float>())).Returns<TestParam>(j=>j.Prop1);
        });
        _builder.OnMethodExecuted(c =>
        {
            list.Add(c.Method);
            if (c.Exception is not null)
            {
                c.Abort = true;
            }
            return Task.CompletedTask;
        });
        var engine = _builder.Build();
        var ex = Assert.ThrowsAsync<VerifierExecutionException>(async () => await engine.TestService());
        Assert.IsTrue(ex.InnerException is InvalidCastException);
        Assert.AreEqual(nameof(TestClient.GetMethod2), ex.Method.Name);
        Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod3)));
    }

    [Test]
    public void default_on_method_executed_callback_abort_if_exception()
    {
        _builder.ConfigureVerify(c =>
        {
            c.Verify(k => k.GetMethod2(Behavior.Transform<string>(h => "wrong"), Behavior.Verify<decimal>(), Behavior.Verify<float>())).Returns<TestParam>(j =>
            {
                throw new InvalidCastException();
            });
        });
        var engine = _builder.Build();
        var ex=Assert.ThrowsAsync<VerifierExecutionException>(async () => await engine.TestService());
        Assert.IsTrue(ex.InnerException.InnerException is InvalidCastException);
        Assert.AreEqual(nameof(TestClient.GetMethod2),ex.Method.Name);
        
        Assert.IsFalse(_client.Data.Keys.Any(x=>x==nameof(TestClient.GetMethod3)));
    }



    [Test]
    public async Task on_method_executing_callback()
    {
        var list = new List<MethodInfo>();

        _builder.OnMethodExecuting(c =>
            {
                list.Add(c.Method);
                return Task.CompletedTask;
            })
        .SetMode(EngineMode.Loose)
        .ConfigureSetup(x =>
            {
                x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>())).Skip();
                x.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
                x.Setup(c => c.GetMethod6(Behavior.Generate<bool?>())).Skip();
                x.Setup(c => c.GetMethod3(Behavior.Generate<TestParam>())).Skip();
                x.Setup(c => c.UpdateMethod1(Behavior.Generate<bool?>())).Skip();
                x.Setup(c => c.GetMethod7(Behavior.Generate<TestParam>())).Skip();
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2, list.Count);
        Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod1)));
    }

    [Test]
    public async Task on_method_executing_callback_abort_if_success()
    {
        var list = new List<MethodInfo>();
        _builder.OnMethodExecuting(c =>
        {
            list.Add(c.Method);
            if (c.Method.Name == nameof(TestClient.GetMethod2))
            {
                c.Abort = true;
            }
            return Task.CompletedTask;
        }).SetMode(EngineMode.Strict)
            .ConfigureSetup(v =>
            {
                v.Setup(c => c.GetMethod5<string, int>(Behavior.Generate<TestParam>(), Behavior.Generate<string>())).Skip();
                v.Setup(c => c.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>()));
                v.Setup(c => c.GetMethod2(Behavior.Generate<string>(), Behavior.Generate<decimal>(), Behavior.Generate<float>()));
            });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2, list.Count);
        Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod3)));
    }

    [Test]
    public async Task strict_mode()
    {
        _builder.SetMode(EngineMode.Strict);
        _builder.ConfigureSetup(x =>
        {
            x.Setup(g => g.GetMethod1(Behavior.Generate<int>(), Behavior.Generate<string>()));
        });
        _builder.ConfigureVerify(x =>
        {
            x.Verify(g => g.GetMethod4(Behavior.Generate<TestParam>()));
        });
        var engine = _builder.Build();
        await engine.TestService();

        Assert.AreEqual(2,_client.Data.Count);
        Assert.IsTrue(_client.Data.Any(x => x.Key == nameof(TestClient.GetMethod1)));
        Assert.IsTrue(_client.Data.Any(x => x.Key == nameof(TestClient.GetMethod4)));
        //Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod1)));
        //Assert.IsFalse(list.Any(x => x.Name == nameof(TestClient.GetMethod4)));
    }

    [Test]
    public async Task generic_method_configured_in_verify()
    {
        _builder.ConfigureVerify(c =>
        {
            c.Verify(b => b.GetMethod5<int, TestParam>(null, 3));
        });
        _builder.SetMode(EngineMode.Strict);
        var engine = _builder.Build();
        await engine.TestService();
        
        Assert.IsTrue(_client.Data.Any(x => x.Key == nameof(TestClient.GetMethod5)));
    }

    [Test]
    public async Task generic_method_configured_in_setup()
    {
        _builder.ConfigureSetup(c =>
        {
            c.Setup(b => b.GetMethod5<int, TestParam>(null, 3));
        });
        _builder.SetMode(EngineMode.Strict);
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(_client.Data.Any(x => x.Key == nameof(TestClient.GetMethod5)));
    }

    [Test]
    public async Task client_method_is_void_but_action_method_return_value()
    {
        _builder.ConfigureVerify(c =>
        {
            c.Verify(b => b.UpdateMethod1(Behavior.Verify<bool?>())).NoReturn();
        });
        _builder.SetMode(EngineMode.Strict);
        var engine = _builder.Build();
        await engine.TestService();

        Assert.IsTrue(_client.Data.Any(x => x.Key == nameof(TestClient.UpdateMethod1)));
    }
    
}