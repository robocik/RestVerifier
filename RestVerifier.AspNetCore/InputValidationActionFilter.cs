using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using RestVerifier.Core;

namespace RestVerifier.AspNetCore;

public class InputValidationActionFilter : IActionFilter
{
    private CompareRequestValidatorInjector _requestValidator;

    public InputValidationActionFilter(CompareRequestValidatorInjector requestValidator)
    {
        _requestValidator = requestValidator;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        var validator = _requestValidator.Validator;
        if (validator != null)
        {
            if (validator.ValidateParams(context.ActionArguments))
            {
                if (context.ActionDescriptor is ControllerActionDescriptor method)
                {

                    Type? returnType = method.MethodInfo.ReturnType;
                    if (returnType == typeof(IActionResult) || returnType == typeof(Task<IActionResult>))
                    {
                        returnType = null;
                    }

                    var returnObj = validator.AddReturnType(returnType);

                    if (returnObj is IActionResult ar)
                    {
                        context.Result = ar;
                    }
                    else
                    {
                        context.Result = new ObjectResult(returnObj)
                        {
                            StatusCode = 200
                        };
                    }

                }
            }
        }
        
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {

    }
}

