using System.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;

namespace RestVerifier.AspNetCore;

public class InputValidationActionFilter : IActionFilter
{
    private CompareRequestValidator _requestValidator;

    public InputValidationActionFilter(CompareRequestValidator requestValidator)
    {
        _requestValidator = requestValidator;
    }

    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (_requestValidator.ValidateParams(context.ActionArguments))
        {
            if (context.ActionDescriptor is ControllerActionDescriptor method)
            {
                if (method.ActionName == "GetWebFormPdf")
                {
                    
                }
                Type? returnType = method.MethodInfo.ReturnType;
                if (returnType == typeof(IActionResult) || returnType == typeof(Task<IActionResult>))
                {
                    returnType = null;
                }

                var returnObj = _requestValidator.AddReturnType(returnType);
                
                // if (method.ActionName == "ExecuteSqlQuery")
                // {
                //     var ds = (DataSet)returnObj;
                //     context.Result = new ObjectResult(ds){
                //         StatusCode = 200
                //     };;
                // }
                // else
                // {
                //     context.Result = new JsonResult(returnObj)
                //     {
                //         StatusCode = 200
                //     };
                // }
                if (returnObj is IActionResult ar)
                {
                    context.Result = ar;
                }
                else
                {
                    context.Result = new ObjectResult(returnObj){
                        StatusCode = 200
                    };    
                }
                
            }
        }
    }

    public void OnActionExecuted(ActionExecutedContext context)
    {

    }
}

