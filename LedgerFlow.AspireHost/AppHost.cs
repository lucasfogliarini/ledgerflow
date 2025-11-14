var builder = DistributedApplication.CreateBuilder(args);

var sqlServer = builder.AddSqlServer("sqlserver", port: 1433)
    .WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume();

var database = sqlServer.AddDatabase("LedgerFlow");

var redis = builder.AddRedis("redis");

var keycloak = builder.AddKeycloakContainer("keycloak", port: 2000)
    .WithDataVolume()
    .WithEnvironment("KEYCLOAK_ADMIN", "admin")
    .WithEnvironment("KEYCLOAK_ADMIN_PASSWORD", "admin");

var transactionApi = "transactions-api";
builder.AddProject<Projects.LedgerFlow_Transactions_WebApi>(transactionApi)
    .WithReference(database).WaitFor(database)
    .WithReference(redis)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: transactionApi, port: 2004);

var ledgersummariesApi = "ledgersummaries-api";
builder.AddProject<Projects.LedgerFlow_LedgerSummaries_WebApi>(ledgersummariesApi)
    .WithReference(database).WaitFor(database)
    .WithReference(redis)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: ledgersummariesApi, port: 2003);

var app = builder.Build();

app.Run();