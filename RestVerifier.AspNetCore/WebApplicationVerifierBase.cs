using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace RestVerifier.AspNetCore;

public class WebApplicationVerifierBase<T> : WebApplicationFactory<T> where T:class
{
    private readonly CompareRequestValidator _requestValidator;

    protected bool SkipAuthentication { get; set; } = true;
    public WebApplicationVerifierBase(CompareRequestValidator requestValidator)
    {
        _requestValidator = requestValidator;
    }
    
     
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {

            services.AddSingleton(_requestValidator);
            if (SkipAuthentication)
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();    
            }
            
            services.AddControllers(options =>
            {

                options.Filters.Add(typeof(InputValidationActionFilter));
                options.Filters.Add(typeof(AllowAnonymousFilter));
            });
        });

    }
}