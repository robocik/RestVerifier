using RestVerifier.AutoFixture;
using RestVerifier.Core;
using RestVerifier.Core.Interfaces;
using RestVerifier.FluentAssertions;

namespace RestVerifier;

public static class Engine
{
    public static RestVerifierEngine<TClient> CreateDefault<TClient>() where TClient:notnull
    {
        var builder= CreateDefaultBuilder<TClient>();
        var engine=builder.Build();
        return engine;
    }

    public static IGlobalSetupStarter<TClient> CreateDefaultBuilder<TClient>() where TClient : notnull
    {
        var builder = RestVerifierEngine<TClient>.Create();
        builder
            .UseComparer<FluentAssertionComparer>()
            .UseObjectCreator<AutoFixtureObjectCreator>();
        return builder;
    }
}