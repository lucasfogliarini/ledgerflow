var builder = WebApplication.CreateBuilder(args);

builder.AddLedgerSummariesWebApi();

var app = builder.Build();

app.Migrate();

app.UseLedgerSummariesWebApi();

app.Run();
