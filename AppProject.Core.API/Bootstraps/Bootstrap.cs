using System;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using AppProject.Core.API.Auth;
using AppProject.Core.API.Middleware;
using AppProject.Core.Contracts;
using AppProject.Core.Infrastructure.Database;
using AppProject.Core.Infrastructure.Database.Entities.Auth;
using AppProject.Core.Infrastructure.Database.Mapper;
using AppProject.Core.Services;
using AppProject.Exceptions;
using Mapster;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Validations;

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

        ConfigureAuthentication(builder);

        ConfigureSwagger(builder);

        return builder;
    }

    public static WebApplication UseApiPipeline(this WebApplication app)
    {
        app.UseRequestLocalization();

        // ambiente de desenvolvimento
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");

                var auth0Options = new Auth0Options();
                app.Configuration.GetSection("Auth0").Bind(auth0Options);

                c.OAuthClientId(auth0Options.ClientId);
                c.OAuthAppName("API - Swagger");
                c.OAuthUsePkce();
                c.OAuthScopeSeparator(" ");

                c.OAuthScopes("openid", "profile", "email", "offline_access");

                c.OAuthAdditionalQueryStringParams(new Dictionary<string, string>
                {
                    { "audience", auth0Options.Audience ?? string.Empty }
                });
            });
        }

        app.UseMiddleware<ExceptionMiddleware>();

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

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

    public static async Task CreateOrUpdateSystemAdminUserAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var applicationDbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var systemAdminUserOptions = new SystemAdminUserOptions();
        app.Configuration.GetSection("SystemAdminUser").Bind(systemAdminUserOptions);

        if (string.IsNullOrWhiteSpace(systemAdminUserOptions.Name)
            || string.IsNullOrWhiteSpace(systemAdminUserOptions.Email))
        {
            throw new ArgumentException("SystemAdminUser configuration is not set properly.");
        }

        var user = await applicationDbContext.Users.FirstOrDefaultAsync(u => u.IsSystemAdmin);

        if (user == null)
        {
            var adminUserId = Guid.NewGuid();

            user = new TbUser
            {
                Id = adminUserId,
                Name = systemAdminUserOptions.Name,
                Email = systemAdminUserOptions.Email,
                IsSystemAdmin = true,
                CreatedAt = DateTime.UtcNow,
                CreatedByUserId = adminUserId,
                CreatedByUserName = systemAdminUserOptions.Name!
            };

            applicationDbContext.Users.Add(user);
            await applicationDbContext.SaveChangesAsync();
        }
        else if (user.Name != systemAdminUserOptions.Name || user.Email != systemAdminUserOptions.Email)
        {
            user.Name = systemAdminUserOptions.Name!;
            user.Email = systemAdminUserOptions.Email!;
            user.UpdatedAt = DateTime.UtcNow;
            user.UpdatedByUserId = user.Id;
            user.UpdatedByUserName = user.Name;

            applicationDbContext.Users.Update(user);
            await applicationDbContext.SaveChangesAsync();
        }
    }

    public static void ConfigureUsers(WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<IUserContext, UserContext>();

        builder.Services.AddHttpContextAccessor();
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
        builder.Services.AddScoped<IDatabaseRepository, DatabaseRepository>();

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

    private static void ConfigureAuthentication(WebApplicationBuilder builder)
    {
        builder.Services.AddAuthorization();

        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        var authority = auth0Options.Authority;
        var audience = auth0Options.Audience;

        if (string.IsNullOrWhiteSpace(authority) || string.IsNullOrWhiteSpace(audience))
        {
            throw new ArgumentException("Auth0 configuration is not set properly");
        }

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.Authority = authority;
            options.Audience = audience;

            options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = authority,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateLifetime = true,
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
    }

    private static void ConfigureSwagger(WebApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();

        var auth0Options = new Auth0Options();
        builder.Configuration.GetSection("Auth0").Bind(auth0Options);

        var autority = auth0Options.Authority;

        if (string.IsNullOrWhiteSpace(autority))
        {
            throw new ArgumentException("Auth0 configuration is not set properly.");
        }

        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "API",
                Version = "v1"
            });

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.OAuth2,
                Flows = new OpenApiOAuthFlows
                {
                    AuthorizationCode = new OpenApiOAuthFlow
                    {
                        AuthorizationUrl = new Uri($"{auth0Options.Authority}/authorize?prompt=login"),
                        TokenUrl = new Uri($"{auth0Options.Authority}/oauth/token"),
                        Scopes = new Dictionary<string, string>
                        {
                            { "openid", "OpenID" },
                            { "profile", "Profile" },
                            { "email", "Email" },
                            { "offline_access", "Offline Access" }
                        }
                    }
                },
                In = ParameterLocation.Header,
                Name = "Authorization",
                Scheme = "Bearer",
                BearerFormat = "JWT",
                Description = "OAuth2 with Auth0"
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    new[] { "openid", "profile", "email", "offline_access" }
                }
            });
        });
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

    private class Auth0Options
    {
        public string? Authority { get; set; }

        public string? ClientId { get; set; }

        public string? Audience { get; set; }
    }

    private class SystemAdminUserOptions
    {
        public string? Name { get; set; }

        public string? Email { get; set; }
    }
}
