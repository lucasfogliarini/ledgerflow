using LedgerFlow.LedgerSummaries.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text.Json;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddWebApi(this WebApplicationBuilder builder)
    {
        builder.AddApplication();
        builder.Services.AddEndpoints();
        builder.AddHealthChecks();
        builder.Services.AddProblemDetails();
        builder.AddOutputCache();
        builder.Services.AddOpenApi();
        builder.AddJwtBearerAuthentication();
    }
    public static void UseWebApi(this WebApplication app)
    {
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseOutputCache();//precisa ser antes do MapEndpoints e depois da authenticação
        app.MapEndpoints();
        app.MapHealthChecks();
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
    }

    public static IServiceCollection AddEndpoints(this IServiceCollection services)
    {
        var endpointTypes = typeof(Program).Assembly
            .DefinedTypes
            .Where(type => !type.IsAbstract
                           && !type.IsInterface
                           && typeof(IEndpoint).IsAssignableFrom(type))
            .Select(type => ServiceDescriptor.Scoped(typeof(IEndpoint), type));

        services.TryAddEnumerable(endpointTypes);

        return services;
    }
    public static IApplicationBuilder MapEndpoints(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        IEnumerable<IEndpoint> endpoints = scope.ServiceProvider.GetRequiredService<IEnumerable<IEndpoint>>();

        foreach (IEndpoint endpoint in endpoints)
        {
            endpoint.MapEndpoint(app).RequireAuthorization();
        }

        return app;
    }
    private static void AddHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddLedgerFlowDbContextCheck();
    }
    private static void MapHealthChecks(this WebApplication app)
    {
        var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true, 
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    version,
                    app.Environment.EnvironmentName,
                    status = report.Status.ToString(),
                    checks = report.Entries.Select(entry => new
                    {
                        name = entry.Key,
                        status = entry.Value.Status.ToString(),
                        description = entry.Value.Description,
                    })
                });

                await context.Response.WriteAsync(result);
            }
        });
    }
    private static void AddOutputCache(this WebApplicationBuilder builder)
    {
        builder.Services.AddOutputCache(options =>
        {
            options.AddPolicy("per-user", policy =>
            {
                policy.VaryByValue(context =>
                {
                    var username = context.User.FindFirst("preferred_username")?.Value ?? "anonymous";
                    return new KeyValuePair<string, string>(nameof(username), username);
                });
                policy.Expire(TimeSpan.FromSeconds(30));
            });
        });
    }
    private static void AddJwtBearerAuthentication(this WebApplicationBuilder builder)
    {
        var jwtSection = builder.Configuration.GetSection(nameof(JwtSettings));
        builder.Services.Configure<JwtSettings>(jwtSection);

        var jwtSettings = jwtSection.Get<JwtSettings>()!;

        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
         .AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
         {
             options.Authority = jwtSettings.Authority;
             options.Audience = jwtSettings.Audience;
             options.RequireHttpsMetadata = false;
             options.SaveToken = true;

             options.TokenValidationParameters = new TokenValidationParameters
             {  
                 // Necessário pois o Keycloak emite issuer com localhost enquanto a API o acessa via host.docker.internal.
                 ValidateIssuer = false
             };
         });
        builder.Services.AddAuthorization();
    }
    public record JwtSettings(string Authority, string Audience);
}