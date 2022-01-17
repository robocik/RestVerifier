using System;
using System.Threading.Tasks;
using NUnit.Framework;
using RestVerifier;

namespace RestVerifierTests;

class MockRestVerifierEngine : RestVerifierEngineBase<TestClient>
{
    protected override Task Invoke(Func<TestClient, Task> action)
    {
        return Task.CompletedTask;
    }
}

[TestFixture]
public class RestVerifierEngineBaseTests
{
    private MockRestVerifierEngine engine = new();

    [SetUp]
    public void Setup()
    {
        engine = new();
    }
    [Test]
    public async Task Test()
    {
        await engine.TestService();
    }
}