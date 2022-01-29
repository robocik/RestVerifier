using System.Text.Json;
using Microsoft.AspNetCore.TestHost;
using NUnit.Framework;
using RestVerifier.AspNetCore;
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
        _builder = Engine.CreateDefaultBuilder<WeatherForecastService>();

        _builder.SetMode(EngineMode.Strict);
        _builder.CreateClient(v =>
        {
            _service.SetCompareRequestValidator(v);
            var httpClient = _service.CreateClient();
            var client = new WeatherForecastService(httpClient);
            return Task.FromResult(client);
        });
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
        Assert.IsTrue(exception!.InnerException is ArgumentOutOfRangeException);
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
        Assert.IsTrue(exception!.InnerException is AssertionException);
    }

    [Test]
    public async Task controller_method_returns_task_PersonDTO_but_in_client_code_we_returns_string()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonName(Behavior.Verify<Guid>())).Returns<PersonDTO>(v=>v.Name);
        });
        var engine = _builder.Build();
        await engine.TestService();
    }

    [Test]
    public async Task controller_method_returns_iactionresult_but_in_client_code_we_returns_string()
    {
        _builder.ConfigureVerify(x =>
        {
            x.Verify(b => b.GetPersonNameAction(Behavior.Verify<Guid>())).Returns<PersonDTO>(v => v.Name);
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
                    return new object?[]{new ParameterValue("address")
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
}
