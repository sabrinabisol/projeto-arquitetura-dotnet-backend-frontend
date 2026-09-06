using System;
using System.Reflection;

namespace AppProject.Core.API.Bootstraps;

public static class Bootstrap
{
    // injecao de dependencias
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        var mvcBuilder = builder.Services.AddControllers();

        ConfigureControllers(mvcBuilder);

        return builder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
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

    private static IEnumerable<Assembly> GetControllersAssemblies() =>
        [
          Assembly.Load("AppProject.Core.Controllers.General"),
        ];
}
