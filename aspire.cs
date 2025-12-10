#:sdk Aspire.AppHost.Sdk@13.0.2
#:package Keycloak.AuthServices.Aspire.Hosting@0.2.0
#:package Aspire.Hosting.SqlServer@13.0.2
#:package Aspire.Hosting.Kafka@13.0.2
#:package Aspire.Hosting.Redis@13.0.2
#:package Aspire.Hosting.NodeJs@9.5.2

using Aspire.Hosting;
using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

builder.Configuration["ASPIRE_ALLOW_UNSECURED_TRANSPORT"] = "true";
builder.Configuration["ASPIRE_DASHBOARD_OTLP_HTTP_ENDPOINT_URL"] = "http://localhost:5001";
builder.Configuration["ASPNETCORE_URLS"] = "http://localhost:5000";
builder.Configuration["Parameters:sqlserver-password"] = "Ledgerflow!123";

// Add Keycloak for authentication
var keycloak = builder.AddKeycloakContainer("keycloak", port: 2000)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_keycloak_data")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithImport("ledgerflow-realm-export.json");

// Add SQL Server database
var sqlServer = builder.AddSqlServer("sqlserver", port: 2001)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_sqlserver_data");
var wolverineDatabase = sqlServer.AddDatabase("WolverineDatabase");

// Add Kafka message broker
var kafka = builder.AddKafka("kafka", port: 2002)
    .WithKafkaUI()
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_kafka_data");

// Add Redis cache
var redis = builder.AddRedis("redis", port: 2003)
    .WithRedisInsight()
    .WithDataVolume("aspire_redis_data");

// Add Transactions API
var transactionApi = "transactions-api";
var transactionsDatabase = sqlServer.AddDatabase("TransactionsDatabase");
var transactionsProject = builder.AddProject(transactionApi, "LedgerFlow.Transactions.WebApi")
    .WithReference(wolverineDatabase).WaitFor(wolverineDatabase)
    .WithReference(transactionsDatabase).WaitFor(transactionsDatabase)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(kafka).WaitFor(kafka);

// Add LedgerSummaries API
var ledgersummariesApi = "ledgersummaries-api";
var ledgerSummariesDatabase = sqlServer.AddDatabase("LedgerSummariesDatabase");
var ledgerSummariesProject = builder.AddProject(ledgersummariesApi, "LedgerFlow.LedgerSummaries.WebApi")
    .WithReference(wolverineDatabase).WaitFor(wolverineDatabase)
    .WithReference(ledgerSummariesDatabase).WaitFor(ledgerSummariesDatabase)
    .WithReference(keycloak).WaitFor(keycloak)
    .WithReference(kafka).WaitFor(kafka)
    .WithReference(redis);

var webApp = builder.AddNpmApp("ledgerflow-web", "LedgerFlow.Web", "dev")
    .WithEnvironment("TRANSACTIONS_API_URL", "http://localhost:3000")
    .WithEnvironment("LEDGERSUMMARIES_API_URL", "http://localhost:3001")
    .WithEnvironment("KEYCLOAK_URL", "http://localhost:2000")
    .WaitFor(transactionsProject)
    .WaitFor(ledgerSummariesProject)
    .WithHttpEndpoint(name: "ledgerflow-web", port: 4000, isProxied: false);

var app = builder.Build();

await app.RunAsync();