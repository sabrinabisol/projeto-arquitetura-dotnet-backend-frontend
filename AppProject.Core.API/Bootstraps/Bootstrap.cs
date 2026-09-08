using System;
using System.Globalization;
using System.Reflection;
using Microsoft.AspNetCore.Localization;

namespace AppProject.Core.API.Bootstraps;

public static class Bootstrap
{
    // injecao de dependencias
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        var mvcBuilder = builder.Services.AddControllers();

        ConfigureControllers(mvcBuilder);

        ConfigureLocalization(builder, mvcBuilder);

        return builder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseRequestLocalization();

        // ambiente de desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        return app;
    }

    private static void ConfigureControllers(IMvcBuilder mvcBuilder)
    {
        foreach (var assembly in GetControllersAssemblies())
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
    }

    public static void ConfigureLocalization(WebApplicationBuilder builder, IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddDataAnnotationsLocalization();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[]{"en-US", "pt-BR", "es-ES"};
            options.DefaultRequestCulture = new RequestCulture("en-US");
            options.SupportedCultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.SupportedUICultures = supportedCultures.Select(c => new CultureInfo(c)).ToList();
            options.RequestCultureProviders = new List<IRequestCultureProvider>
            {
                new QueryStringRequestCultureProvider(),
                new CookieRequestCultureProvider(),
                new AcceptLanguageHeaderRequestCultureProvider()
            };
        });
    }

    private static IEnumerable<Assembly> GetControllersAssemblies() =>
        [
          Assembly.Load("AppProject.Core.Controllers.General"),
        ];
}
