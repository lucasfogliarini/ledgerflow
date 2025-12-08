#:sdk Aspire.AppHost.Sdk@13.0.2
#:package Aspire.Hosting.SqlServer@13.0.2
#:package Aspire.Hosting.Redis@13.0.2
#:package Aspire.Hosting.NodeJs@9.5.2
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0

using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";
builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"] = "http://localhost:2007";
builder.Configuration["ASPNETCORE_URLS"] = "http://localhost:2006";
builder.Configuration["Parameters:sqlserver-password"] = "Ledgerflow!123";

// Add Keycloak for authentication
var keycloak = builder.AddKeycloakContainer("keycloak", port: 2000)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_keycloak_data")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithImport("ledgerflow-realm-export.json");

// Add SQL Server database
var database = builder.AddSqlServer("sqlserver", port: 2001)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_sqlserver_data")
    .AddDatabase("LedgerFlow");

// Add Redis cache
var redis = builder.AddRedis("redis")
    .WithRedisInsight()
    .WithDataVolume("aspire_redis_data");

// Add Transactions API
var transactionApi = "transactions-api";
var transactionsProject = builder.AddProject(transactionApi, "LedgerFlow.Transactions.WebApi")
    .WithReference(database).WaitFor(database)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithHttpEndpoint(name: transactionApi, port: 2002);

// Add LedgerSummaries API
var ledgersummariesApi = "ledgersummaries-api";
var ledgerSummariesProject = builder.AddProject(ledgersummariesApi, "LedgerFlow.LedgerSummaries.WebApi")
    .WithReference(database).WaitFor(database)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(redis)
    .WithHttpEndpoint(name: ledgersummariesApi, port: 2003);

var webApp = builder.AddNpmApp("ledgerflow-web", "LedgerFlow.Web", "dev")
    .WithEnvironment("NEXT_PUBLIC_TRANSACTIONS_API_URL", "http://localhost:2002")
    .WithEnvironment("NEXT_PUBLIC_LEDGERSUMMARIES_API_URL", "http://localhost:2003")
    .WithEnvironment("NEXT_PUBLIC_KEYCLOAK_URL", "http://localhost:2000")
    .WaitFor(transactionsProject)
    .WaitFor(ledgerSummariesProject)
    .WithHttpEndpoint(name: "ledgerflow-web", port: 2005, isProxied: false);

var app = builder.Build();

await app.RunAsync();