using LedgerFlow;
using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;
using LedgerFlow.Application.Transactions;
using Microsoft.Extensions.Hosting;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddApplication(this IHostApplicationBuilder builder)
    {
        builder.Services.AddHandlers(Assembly.GetExecutingAssembly());
        builder.AddInfrastructure();
    }

    public static void AddLedgerSummariesModule(this IHostApplicationBuilder builder)
    {
        var ledgerSummariesModule = typeof(LedgerSummariesModule).Namespace;
        builder.Services.AddHandlers(Assembly.GetExecutingAssembly(), ledgerSummariesModule);
        builder.AddInfrastructure();
    }
    public static void AddTransactionsModule(this IHostApplicationBuilder builder)
    {
        var transactionsModule = typeof(TransactionsModule).Namespace;
        builder.Services.AddHandlers(Assembly.GetExecutingAssembly(), transactionsModule);
        builder.AddInfrastructure();
    }

    private static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly, string? module = null)
    {
        var handlerInterfaces = new[]
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IDomainEventHandler<>)
        };

        var handlerTypes = assembly.GetTypes();
        if (!string.IsNullOrEmpty(module))
            handlerTypes = handlerTypes.Where(t => t.Namespace == module).ToArray();

        foreach (var type in handlerTypes)
        {
            if (type.IsAbstract || type.IsInterface)
                continue;

            var interfaces = type.GetInterfaces()
                .Where(i => i.IsGenericType && handlerInterfaces.Contains(i.GetGenericTypeDefinition()));

            foreach (var _interface in interfaces)
            {
                services.AddTransient(_interface, type);
            }
        }

        return services;
    }
}
