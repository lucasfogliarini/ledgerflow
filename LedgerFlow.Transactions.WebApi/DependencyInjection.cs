using LedgerFlow.Transactions.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddTransactionsWebApi(this WebApplicationBuilder builder)
    {
        builder.Services.AddTransactionsModule();
        builder.AddInfrastructure();
        builder.Services.AddEndpoints();
        builder.Services.AddProblemDetails();
        builder.AddCors();
        builder.Services.AddOpenApi();
        builder.AddJwtBearerAuthentication();
    }
    public static void UseTransactionsWebApi(this WebApplication app)
    {
        app.UseCors();
        app.UseRateLimiter();//para não autenticados
        app.UseAuthentication();
        app.UseAuthorization();
        //app.UseRateLimiter();//para autenticados. Removido para fazer teste de carga.
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
            endpoint.MapEndpoint(app)
                .RequireAuthorization()
                .RequireRateLimiting("per-user");
        }

        return app;
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
    private static void AddCors(this WebApplicationBuilder builder)
    {
        var corsSettings = builder.Configuration
            .GetSection(nameof(CorsSettings))
            .Get<CorsSettings>()
            ?? throw new InvalidOperationException(
                $"As configurações de CORS ({nameof(CorsSettings)}) não foram encontradas."
            );

        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins(corsSettings.AllowedOrigins)
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
    }
    public record CorsSettings(string[] AllowedOrigins);
}