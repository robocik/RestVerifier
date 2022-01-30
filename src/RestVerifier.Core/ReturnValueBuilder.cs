using System;
using System.Collections;
using System.Collections.Generic;
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
    public object? AddReturnType(MethodConfiguration methodConfig, Type? type)
    {


        //if (type is not null)
        //{
        //    methodConfig.ReturnType = type;
        //}
        methodConfig.ReturnType = methodConfig.ReturnType!.GetTypeWithoutTask();

        return GenerateReturnValue(methodConfig, type);
    }

    private object? GenerateReturnValue(MethodConfiguration methodConfig,Type? actionType)
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
            bool returnTransformInvoked = false;
            returnObject = _requestValidator.Creator.Create(methodConfig.ReturnType!);
            valueToValidate = returnObject;
            if (methodConfig.ReturnTransform != null)
            {
                returnTransformInvoked = true;
                returnObject = (object?)methodConfig.ReturnTransform.DynamicInvoke(returnObject);
            }
            else
            {
                var transform = _configuration.GetReturnTransform(methodConfig.ReturnType!);
                if (transform != null)
                {
                    returnTransformInvoked = true;
                    returnObject = (object?)transform.DynamicInvoke(returnObject);
                }
            }

            if (!returnTransformInvoked && actionType != null)
            {
                var returnType= getTypeToCheck(methodConfig.ReturnType!);
                actionType = getTypeToCheck(actionType);
                if ( actionType != returnType)
                {
                    throw new InvalidCastException(
                        $"Client method {methodConfig.MethodInfo} returns different type than Action method but there is no Return transformation defined for type {actionType}->{returnType}");
                }
                
            }
        }
        else if(!methodConfig.ShouldNotReturnValue && actionType != null &&  !actionType.IsVoid())
        {
            throw new InvalidCastException(
                $"Client method {methodConfig.MethodInfo} returns void type but Action method returns type {actionType}. If this is correct, please mark this method as NoReturn()");
        }


        

        _requestValidator.Context.AddReturnValue(valueToValidate);
        return returnObject;
    }

    Type? getTypeToCheck(Type actionType)
    {
        actionType = actionType.GetTypeWithoutTask();
        if (IsEnumerableType(actionType))
        {
            if (actionType.IsGenericType)
            {
                return actionType.GetGenericArguments()[0];
            }

            return null;//do not check not generic collections
        }
        return actionType;
    }

    bool IsEnumerableType(Type type)
    {
        return type!=typeof(string) && (type.GetInterface(nameof(IEnumerable)) != null);
    }
}