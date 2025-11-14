var builder = WebApplication.CreateBuilder(args);

builder.AddTransactionsWebApi();

var app = builder.Build();

app.Migrate();

app.UseTransactionsWebApi();

app.Run();
