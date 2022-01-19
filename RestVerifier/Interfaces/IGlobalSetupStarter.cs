using System;
using System.Reflection;

namespace RestVerifier.Interfaces;

public interface IGlobalSetupStarter<TClient>
{
    IGlobalSetupStarter<TClient> GetMethods(Func<Type, MethodInfo[]> method);

    IGlobalSetupStarter<TClient> VerifyParameter(Action<ParameterInfo, ParameterValue> method);

    IGlobalSetupStarter<TClient> UseComparer<T>() where T : IObjectsComparer;

    IGlobalSetupStarter<TClient> UseComparer<T>(T comparer) where T : IObjectsComparer;

    IGlobalSetupStarter<TClient> UseObjectCreator<T>() where T : ITestObjectCreator;

    IGlobalSetupStarter<TClient> UseObjectCreator<T>(T objCreator) where T : ITestObjectCreator;

    IGlobalSetupStarter<TClient> ConfigureVerify(Action<IVerifyStarter<TClient>> config);

    IGlobalSetupStarter<TClient> ConfigureSetup(Action<ISetupStarter<TClient>> config);

    RestVerifierEngineBase<TClient> Build();

    IGlobalSetupStarter<TClient> CreateClient(Func<CompareRequestValidator, TClient> factory);
    IGlobalSetupStarter<TClient> ReturnTransform<T>(Func<T, object> func);

    IGlobalSetupStarter<TClient> ParameterTransform<T>(Func<T, object> func);

    IGlobalSetupStarter<TClient> OnMethodExecuted(Action<ExecutionContext> func);
}