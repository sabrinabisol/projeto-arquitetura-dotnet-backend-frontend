using System;
using System.Globalization;
using System.Reflection;
using AppProject.Core.API.Auth;
using AppProject.Core.API.Middleware;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Mapper;
using AppProject.Core.Services;
using AppProject.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AppProject.Core.API.Bootstraps;

public static class Bootstrap
{
    // injecao de dependencias
    public static WebApplicationBuilder AddApiServices(this WebApplicationBuilder builder)
    {
        var mvcBuilder = builder.Services.AddControllers();

        ConfigureControllers(mvcBuilder);

        ConfigureLocalization(builder, mvcBuilder);

        builder.Services.Configure<ApiBehaviorOptions>(options =>
        {
            ConfigureValidation(options);
        });

        ConfigureServices(builder);

        ConfigureUsers(builder);

        ConfigureMapper(builder);

        ConfigureDatabase(builder);

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

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.MapControllers();

        return app;
    }

    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        // Crie o banco de dados e verificar se está com a estrutura correta, se não estiver, ele cria.
        await applicationDbContext.Database.MigrateAsync();
    }

    public static void ConfigureUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();
    }

    public static void ConfigureLocalization(WebApplicationBuilder builder, IMvcBuilder mvcBuilder)
    {
        mvcBuilder.AddDataAnnotationsLocalization();

        builder.Services.Configure<RequestLocalizationOptions>(options =>
        {
            var supportedCultures = new[] { "en-US", "pt-BR", "es-ES" };
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

    private static void ConfigureMapper(WebApplicationBuilder builder)
    {
        builder.Services.AddMapster();
        builder.Services.Scan(scan => scan
            .FromAssemblyOf<IRegisterMapsterConfig>()
            .AddClasses(classes => classes.AssignableTo<IRegisterMapsterConfig>())
            .As<IRegisterMapsterConfig>()
            .WithSingletonLifetime());

        var provider = builder.Services.BuildServiceProvider();
        var configs = provider.GetServices<IRegisterMapsterConfig>();

        var config = TypeAdapterConfig.GlobalSettings;

        foreach (var mapConfig in configs)
        {
            mapConfig.Register(config);
        }

        builder.Services.AddSingleton(config);
    }

    private static void ConfigureControllers(IMvcBuilder mvcBuilder)
    {
        foreach (var assembly in GetControllersAssemblies())
        {
            mvcBuilder.AddApplicationPart(assembly);
        }
    }

    private static void ConfigureValidation(ApiBehaviorOptions options)
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            var modelErrors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .Select(e => e.Value!.Errors.Select(er => er.ErrorMessage));

            var errors = modelErrors.Any() ? string.Join(" ", modelErrors) : null;
            throw new AppException(ExceptionCode.RequestValidation, errors);
        };
    }

    private static void ConfigureServices(WebApplicationBuilder builder)
    {
        builder.Services.Scan(x =>
            x.FromAssemblies(GetServicesAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<ITransientService>())
                .AsImplementedInterfaces()
                .WithTransientLifetime());

        builder.Services.Scan(x =>
            x.FromAssemblies(GetServicesAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<IScopedService>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

        builder.Services.Scan(x =>
            x.FromAssemblies(GetServicesAssemblies())
                .AddClasses(y =>
                    y.AssignableTo<ISingletonService>())
                .AsImplementedInterfaces()
                .WithSingletonLifetime());
    }

    private static void ConfigureDatabase(WebApplicationBuilder builder)
    {
        var connectionStringsOptions = new ConnectionStringsOptions();
        builder.Configuration.GetSection("ConnectionStrings").Bind(connectionStringsOptions);

        var databaseConnection = connectionStringsOptions.DatabaseConnection;
        if (string.IsNullOrWhiteSpace(databaseConnection))
        {
            throw new ArgumentException("Database connection string is not configured.");
        }

        builder.Services.AddDbContext<ApplicationDbContext>(x =>
            x.UseSqlServer(
                databaseConnection,
                y => y.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery))
            .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking));
    }

    private static IEnumerable<Assembly> GetControllersAssemblies() =>
        [
          Assembly.Load("AppProject.Core.Controllers.General"),
        ];

    private static IEnumerable<Assembly> GetServicesAssemblies() =>
        [
          Assembly.Load("AppProject.Core.Services"),
          Assembly.Load("AppProject.Core.Services.General")
        ];

    private class ConnectionStringsOptions
    {
        public string? DatabaseConnection { get; set; }
    }
}
