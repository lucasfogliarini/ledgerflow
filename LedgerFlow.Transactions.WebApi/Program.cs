var builder = WebApplication.CreateBuilder(args);

builder.AddTransactionsWebApi();

var app = builder.Build();

app.UseTransactionsWebApi();

app.Run();
