ARG ASPNET_VERSION=10.0

# Etapa de build e publicação
FROM mcr.microsoft.com/dotnet/sdk:${ASPNET_VERSION} AS build

WORKDIR /app

COPY . .

RUN dotnet publish "LedgerFlow.Transactions.WebApi/LedgerFlow.Transactions.WebApi.csproj" -c Release -o /app/publish

# Etapa final com imagem de runtime
FROM mcr.microsoft.com/dotnet/aspnet:${ASPNET_VERSION} AS runtime
WORKDIR /app

COPY --from=build /app/publish .

EXPOSE 8080
ENTRYPOINT ["dotnet", "transactions-api.dll"]
