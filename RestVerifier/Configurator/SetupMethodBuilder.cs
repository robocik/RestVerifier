using RestVerifier.Interfaces;

namespace RestVerifier.Configurator;

public class SetupMethodBuilder : ISetupMethod
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