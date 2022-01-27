using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using RestVerifier.Core;


namespace RestVerifier.AspNetCore;

public class CompareRequestValidatorInjector
{
    public CompareRequestValidator? Validator { get; set; }
}

public class WebApplicationVerifierBase<T> : WebApplicationFactory<T> where T:class
{
    private readonly CompareRequestValidatorInjector _validatorInjector = new ();
    public bool SkipAuthentication { get; set; } = true;

    public bool DisableModelValidation { get; set; } = true;

    public void SetCompareRequestValidator(CompareRequestValidator requestValidator)
    {
        _validatorInjector.Validator = requestValidator;
    }
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {

        builder.ConfigureServices(services =>
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = DisableModelValidation;
            });
            services.AddSingleton(_validatorInjector);
            services.AddScoped(s =>
            {
                var injector = s.GetService<CompareRequestValidatorInjector>();
                return injector.Validator;
            });

            if (SkipAuthentication)
            {
                services.AddSingleton<IAuthorizationHandler, AllowAnonymous>();    
            }
            
            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(InputValidationActionFilter));

                if (SkipAuthentication)
                {
                    options.Filters.Add(typeof(AllowAnonymousFilter));
                }
                
            });
        });
        base.ConfigureWebHost(builder);
    }
}