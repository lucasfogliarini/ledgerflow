var builder = WebApplication.CreateBuilder(args);

builder.AddLedgerSummariesWebApi();

var app = builder.Build();

app.UseLedgerSummariesWebApi();

app.Run();
