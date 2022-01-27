using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using AutoFixture;
using NUnit.Framework;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.NUnit;

[TestFixture]
public abstract class TestCommunicationBase<TClient> where TClient:notnull
{
    protected IGlobalSetupStarter<TClient> _builder=null!;

    [OneTimeSetUp]
    public virtual void CreateFixture()
    {
        _builder = Engine.CreateDefaultBuilder<TClient>();
        
        ConfigureVerifier(_builder);
    }

    protected virtual void ConfigureVerifier(IGlobalSetupStarter<TClient> builder)
    {

    }

    [Test, TestCaseSource(nameof(MethodTests))]
    public virtual async Task TestServices(MethodInfo method)
    {
        _builder.GetMethods(b => GetMethodFunc(method));
        var engine = _builder.Build();
        await engine.TestService();

    }
    
    public static MethodInfo[] GetMethodFunc(MethodInfo? filter = null)
    {
        var methods = typeof(TClient)
            .GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public)
            .Where(m => !m.IsSpecialName);
        if (filter != null)
        {
            methods = methods.Where(x => x == filter);
        }

        return methods.ToArray();
    }

    public static IEnumerable<TestCaseData> MethodTests()
    {
        var methods = GetMethodFunc();
        foreach (var methodInfo in methods)
        {
            var data = new TestCaseData(methodInfo);
            //data.SetName(methodInfo.Name);
            yield return data;
        }
    }
}