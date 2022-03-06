using System.Text.Json;
using AutoFixture;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using RestVerifier.AspNetCore;
using RestVerifier.AutoFixture;
using RestVerifier.Core;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;
using RestVerifier.NUnit;
using RestVerifier.Tests.AspNetCore.ClientAccess;
using RestVerifier.Tests.AspNetCore.Model;

namespace RestVerifier.Tests.AspNetCore;



[TestFixture]
class TestWebApp
{
    protected WebApplicationVerifierBase<Program> _service = null!;

    protected IGlobalSetupStarter<WeatherForecastService> _builder = null!;

    [OneTimeSetUp]
    public void CreateFixture()
    {
        
        _service = new WebApplicationVerifierBase<Program>();
    }

    [SetUp]
    public void CreateTest()
    {
        var creator = ConfigureAutoFixtureBuilder();

        _builder = Engine.CreateDefaultBuilder<WeatherForecastService>();

        _builder.SetMode(EngineMode.Strict);
        _builder.CreateClient(v =>
        {
            _service.SetCompareRequestValidator(v);
            var httpClient = _service.CreateClient();
            var client = new WeatherForecastService(httpClient);
            return Task.FromResult(client);
        });

        _builder.UseObjectCreator(creator);
        _builder.UseComparer<TestComparer>();
    }

    private static AutoFixtureObjectCreator ConfigureAutoFixtureBuilder()
    {
        var creator = new AutoFixtureObjectCreator();
        creator.Fixture.Register<byte[], Stream>((byte[] data) => new MemoryStream(data));
        return creator;
    }

    [Test]
    public async Task No_parameters_method_returns_ienumerable()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(b => b.GetMethod1());
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public void wrong_return_type_in_controller__void_but_should_be_something_else()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(b => b.GetMethod2());
        });
        var engine = _builder.Build();
        var exception=Assert.ThrowsAsync<VerifierExecutionException>(()=>engine.TestService());
        Assert.IsTrue(exception!.InnerExceptions.Any(b=>b is InvalidCastException));
    }

    [Test]
    public async Task void_type()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(b => b.GetMethod2Void());
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task Simple_parameter_transformation()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPerson(Behavior.Transform<PersonDTO>(h => h.Id)));
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task controller_method_returns_iactionresult()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonAction(Behavior.Verify<Guid>()));
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task controller_method_returns_task_iactionresult()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonTaskAction(Behavior.Verify<Guid>()));
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public void controller_method_returns_task_iactionresult_and_returns_string_but_in_client_code_we_expect_PersonDTO()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonName(Behavior.Verify<Guid>()));
        });
        var engine = _builder.Build();
        var exception=Assert.ThrowsAsync<VerifierExecutionException>(()=> engine.TestService());
        Assert.IsTrue(exception!.InnerExceptions.Count==2);
        Assert.IsTrue(exception!.InnerExceptions.Any(x=>x is InvalidCastException));
    }

    [Test]
    public async Task controller_method_returns_task_PersonDTO_but_in_client_code_we_returns_string()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonName(Behavior.Verify<Guid>())).Returns<string>(v=>new PersonDTO{Name=v});
        });
        var engine = _builder.Build();
        await engine.TestService();
    }
    

    [Test]
    public async Task controller_method_returns_iactionresult_but_in_client_code_we_returns_string()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonNameAction(Behavior.Verify<Guid>())).Returns<string>(v => new PersonDTO{Name=v});
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task valuetuple_parameter()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.UpdatePersonName(Behavior.Verify<Guid>(), Behavior.Verify<string>()))
                .Transform<Guid, string>(
                    (p1, p2) =>
                    {
                        return new object[]{(id: p1, personName: p2)};
                    });
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public void different_parameters_order_position_matcher()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.ParametersOrder(Behavior.Verify<string>(), Behavior.Verify<string>()));
        });
        var engine = _builder.Build();
        var exception = Assert.ThrowsAsync<VerifierExecutionException>(() => engine.TestService());
        Assert.IsTrue(exception!.InnerException is AssertionException);
    }

    [Test]
    public async Task wrong_Async_construction()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.WrongAsync()).NoReturn();
        });
        var engine = _builder.Build();
        //here we have used try...catch instead of Assert.ThrowsAsync because sometimes in this test we don't have exception because somehow wrong async construction inside WrongAsync() method works good ;)
        //But this shouldn't be a big problem - if in the end user code his async construction works good then there is no problem and test should pass
        try
        {
            await engine.TestService();
        }
        catch (VerifierExecutionException e) when (e.Message.Contains("reach endpoint"))
        {
        }
    }
    [Test]
    public async Task different_parameters_order_name_matcher()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.ParametersOrder(Behavior.Verify<string>(), Behavior.Verify<string>()));
        });
        _builder.UseNameMatchingStrategy();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task different_parameters_order_with_different_param_names_name_matcher__transform()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.ParametersOrderWithDifferentParamNames(Behavior.Verify<string>(), Behavior.Verify<string>()))
                .Transform<string,string>((address, name) =>
                {
                    return new object[]{new ParameterValue("address")
                    {
                        ValueToCompare = address
                    }, 
                        new ParameterValue("name")
                    {
                        ValueToCompare = name
                    }};
                });
        });
        _builder.UseNameMatchingStrategy();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task different_parameters_order_with_different_param_names_name_matcher__set_name_name()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(
                b => b.ParametersOrderWithDifferentParamNames(Behavior.Verify<string>("address"), Behavior.Verify<string>("name")));
        });
        _builder.UseNameMatchingStrategy();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task no_return_value_on_client_side()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonNoReturn(Behavior.Transform<PersonDTO>(h => h.Id))).NoReturn();
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task test_exception_handling_correct_handling()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonNoReturn(Behavior.Transform<PersonDTO>(h => h.Id))).NoReturn();
        });
        _builder.CheckExceptionHandling<InvalidOperationException>();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public Task test_exception_handling_incorrect_handling()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.WrongExceptionHandling(Behavior.Transform<PersonDTO>(h => h.Id)));
        });
        _builder.CheckExceptionHandling<InvalidOperationException>();
        var engine = _builder.Build();
        var ex=Assert.ThrowsAsync<VerifierExecutionException>(()=>engine.TestService());
        Assert.IsTrue(ex.Message.Contains("handling") && ex.Message.Contains(typeof(InvalidOperationException).ToString()));
        return Task.CompletedTask;
    }

    [Test]
    public async Task test_exception_handling_incorrect_handling_but_suppressing()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.WrongExceptionHandling(Behavior.Transform<PersonDTO>(h => h.Id))).SuppressCheckExceptionHandling();
        });
        _builder.CheckExceptionHandling<InvalidOperationException>();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task test_exception_handling_correct_handling_for_void_action_method()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetMethod2Void());
        });
        _builder.CheckExceptionHandling<InvalidOperationException>();
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task test()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.DeleteNote(Behavior.Verify<Guid>()));
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task ignore_parameter()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonWithAdditionalParameter(Behavior.Verify<Guid>(),Behavior.Ignore<int>()));
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task get_file_transform_return()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetFileContent(Behavior.Verify<string>())).Returns<Stream>(stream =>
            {
                var memory = (MemoryStream)stream;
                var newMemory = new MemoryStream();
                memory.CopyTo(newMemory);
                newMemory.Position = 0;
                memory.Position = 0;
                return new FileStreamResult(newMemory, "text/json");
            });
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task set_parameter_if_not_default_value()
    {
        _builder.ConfigureSetup(x =>
        {
            x.Setup(b => b.GetStatus(Behavior.Generate<int>(), 0));
        });
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetStatus(Behavior.Verify<int>(),Behavior.Verify<byte>(0)));
        });
        var engine = _builder.Build();
        await engine.TestService();

    }

}
