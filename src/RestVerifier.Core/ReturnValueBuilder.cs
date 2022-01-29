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
       

        if (type is not null)
        {
            methodConfig.ReturnType = type;
        }
        methodConfig.ReturnType = methodConfig.ReturnType!.GetTypeWithoutTask();

        return GenerateReturnValue(methodConfig);
    }

    private object? GenerateReturnValue(MethodConfiguration methodConfig)
    {
        var parameter = methodConfig.ReturnTransform?.Method.GetParameters().SingleOrDefault();
        if (parameter != null)
        {
            methodConfig.ReturnType = parameter.ParameterType;
        }

        object? returnObject = ValidationContext.NotSet;
        object? valueToValidate = ValidationContext.NotSet;
        if (!methodConfig.ReturnType.IsVoid())
        {
            returnObject=_requestValidator.Creator.Create(methodConfig.ReturnType!);
            valueToValidate = returnObject;
            if (methodConfig.ReturnTransform != null)
             {

                 valueToValidate = (object?)methodConfig.ReturnTransform.DynamicInvoke(returnObject);
             }
             else
             {
                 var transform = _configuration.GetReturnTransform(methodConfig.ReturnType!);
                 if (transform != null)
                 {
                     valueToValidate = (object?)transform.DynamicInvoke(returnObject);
                 }
             }
        }
        
        

        

        _requestValidator.Context.AddReturnValue(valueToValidate);
        return returnObject;
    }
}