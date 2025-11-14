var builder = DistributedApplication.CreateBuilder(args);

var database = builder.AddSqlServer("sqlserver", port: 1433)
    //.WithLifetime(ContainerLifetime.Persistent)
    .WithDataVolume("aspire_sqlserver_data")
    .AddDatabase("LedgerFlow");

var redis = builder.AddRedis("redis")
    .WithDataVolume("aspire_redis_data");

var keycloak = builder.AddKeycloakContainer("keycloak", port: 2000)
    .WithDataVolume("aspire_keycloak_data")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_USERNAME", "admin")
    .WithEnvironment("KC_BOOTSTRAP_ADMIN_PASSWORD", "admin")
    .WithImport("import");

var transactionApi = "transactions-api";
builder.AddProject<Projects.LedgerFlow_Transactions_WebApi>(transactionApi)
    .WithReference(database).WaitFor(database)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: transactionApi, port: 2002);

var ledgersummariesApi = "ledgersummaries-api";
builder.AddProject<Projects.LedgerFlow_LedgerSummaries_WebApi>(ledgersummariesApi)
    .WithReference(database).WaitFor(database)
    .WithReference(redis)
    .WithReference(keycloak)
    .WithHttpEndpoint(name: ledgersummariesApi, port: 2003);

var app = builder.Build();

app.Run();