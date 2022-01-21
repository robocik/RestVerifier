namespace RestVerifier.Core
{
    public interface IRemoteServiceContext
    {
        void AddParameters(params ParameterValue[] param);
        void AddReturnValue(object? value);

        void AddValues(params object?[] param);
    }
}