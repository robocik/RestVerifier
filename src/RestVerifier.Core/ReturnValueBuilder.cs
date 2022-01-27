using System;
using System.Linq;
using System.Threading.Tasks;
using RestVerifier.Core.Configurator;
using RestVerifier.Core.Interfaces;

namespace RestVerifier.Core;

public sealed class ReturnValueBuilder
{
    private readonly VerifierConfiguration _configuration;
    private readonly CompareRequestValidator _requestValidator;

    public ReturnValueBuilder(VerifierConfiguration configuration, CompareRequestValidator requestValidator)
    {
        _configuration = configuration;
        _requestValidator = requestValidator;
    }
    public object? AddReturnType(MethodConfiguration methodConfig,Type? type)
    {
        methodConfig.ReturnType ??= type;
        if (methodConfig.ReturnType.IsVoid())
        {
            return null;
        }

        methodConfig.ReturnType = methodConfig.ReturnType!.GetTypeWithoutTask();

        return GenerateReturnValue(methodConfig);
    }

    private object? GenerateReturnValue(MethodConfiguration methodConfig)
    {

        var returnObject = _requestValidator.Creator.Create(methodConfig.ReturnType!);
        _requestValidator.Context.AddReturnValue(returnObject);

        if (methodConfig.ReturnTransform != null)
        {
            returnObject = (object?)methodConfig.ReturnTransform.DynamicInvoke(returnObject);
        }
        else
        {
            var transform = _configuration.GetReturnTransform(methodConfig.ReturnType!);
            if (transform != null)
            {
                returnObject = (object?)transform.DynamicInvoke(returnObject);
            }
        }
        return returnObject;
    }
}