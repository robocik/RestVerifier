using System.Data;
using System.Net;
using Microsoft.AspNetCore.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using RestVerifier.Tests.AspNetCore.Model;

namespace RestVerifier.Tests.AspNetCore;

public static class ExceptionMiddlewareExtensions
{
    public static void ConfigureExceptionHandler(this IApplicationBuilder app)
    {
        app.UseExceptionHandler(appError =>
        {
            appError.Run(async context =>
            {
                var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
                var exception = contextFeature!.Error;

                var errorDetails = new ErrorDetails()
                {
                    Message = exception.Message
                };
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                switch (exception)
                {
                    case ObjectNotFoundException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.ServiceError = ServiceError.ObjectNotFoundException;
                        break;

                    case UnauthorizedAccessException:
                        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
                        errorDetails.ServiceError = ServiceError.UnauthorizedAccessException;
                        break;
                    case ArgumentNullException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.ServiceError = ServiceError.ArgumentNullException;
                        break;
                    case InvalidOperationException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.ServiceError = ServiceError.InvalidOperationException;
                        break;
                    case ArgumentOutOfRangeException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.ServiceError = ServiceError.ArgumentOutOfRangeException;
                        break;
                    case ConstraintException:
                        context.Response.StatusCode = (int)HttpStatusCode.Conflict;
                        errorDetails.ServiceError = ServiceError.ConstraintException;
                        break;
                    case AssertionException:
                        context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        errorDetails.ServiceError = ServiceError.AssertException;
                        break;

                }



                context.Response.ContentType = "application/json";
                var json = JsonConvert.SerializeObject(errorDetails);
                await context.Response.WriteAsync(json);
            });
        });

    }
}