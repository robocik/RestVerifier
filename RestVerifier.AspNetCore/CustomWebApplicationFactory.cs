﻿using System.Reflection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using RestVerifier.Core;

namespace RestVerifier.AspNetCore;

public class CustomWebApplicationFactory<TStartup>
    : WebApplicationVerifierBase<TStartup> where TStartup : class
{

    protected override IHost CreateHost(IHostBuilder builder)
    {
        builder.ConfigureServices(services =>
        {
        }).UseContentRoot(".");
        return base.CreateHost(builder);
    }

    protected override IHostBuilder CreateHostBuilder()
    {
        return Host.CreateDefaultBuilder(new string[] { "renti" })
            .ConfigureServices(services =>
            {

            }).ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<TStartup>().UseSetting(WebHostDefaults.ApplicationKey, typeof(TStartup).GetTypeInfo().Assembly.FullName);
            });

    }


    public CustomWebApplicationFactory(CompareRequestValidator? requestValidator) : base(requestValidator)
    {
    }
}