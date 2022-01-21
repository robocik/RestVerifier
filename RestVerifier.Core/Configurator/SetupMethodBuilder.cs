using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core.Configurator;

sealed class SetupMethodBuilder : ISetupMethod
{
    private readonly MethodConfiguration _methodConfig;

    public SetupMethodBuilder(MethodConfiguration methodConfig)
    {
        _methodConfig = methodConfig;
    }

    void ISetupMethod.Skip()
    {
        _methodConfig.Skip = true;
    }
}