using LedgerFlow.Infrastructure;
using LedgerFlow.LedgerSummaries.WebApi;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.IdentityModel.Tokens;
using StackExchange.Redis;
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
        builder.AddCors();
        builder.Services.AddOpenApi();
        builder.AddJwtBearerAuthentication();
        builder.AddDistributedCache();
    }
    public static void UseWebApi(this WebApplication app)
    {
        app.UseCors();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseRateLimiter();
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
    private static void AddHealthChecks(this WebApplicationBuilder builder)
    {
        builder.Services.AddLedgerFlowDbContextCheck();
    }
    private static void MapHealthChecks(this WebApplication app)
    {
        var serviceInfo = ServiceInfo.Get();
        app.MapHealthChecks("/health", new HealthCheckOptions
        {
            Predicate = _ => true, 
            ResponseWriter = async (context, report) =>
            {
                context.Response.ContentType = "application/json";

                var result = JsonSerializer.Serialize(new
                {
                    serviceInfo.Name,
                    serviceInfo.Version,
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
    public static void AddDistributedCache(this WebApplicationBuilder builder)
    {
        var redisSettings = builder.Configuration
            .GetSection(nameof(RedisSettings))
            .Get<RedisSettings>();

        if (redisSettings is null || string.IsNullOrWhiteSpace(redisSettings.Configuration) || !IsRedisAvailable(redisSettings.Configuration))
        {
            builder.Services.AddDistributedMemoryCache();
            return;
        }

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = redisSettings.Configuration;
            options.InstanceName = redisSettings.InstanceName ?? "LedgerFlow:";
        });
        builder.Services.AddHealthChecks().AddRedis(redisSettings.Configuration, "redis-ledgerflow");
    }
    private static bool IsRedisAvailable(string configuration)
    {
        var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
        try
        {
            using var connection = ConnectionMultiplexer.Connect(configuration);
            var db = connection.GetDatabase();
            var pong = db.Ping();
            logger.LogInformation("Redis respondeu ao ping em {Elapsed} ms.", pong.TotalMilliseconds);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Falha ao conectar ao Redis ({Config})", configuration);
            return false;
        }
    }
    public record RedisSettings(string? Configuration, string? InstanceName = null);
}