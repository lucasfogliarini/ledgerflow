using LedgerFlow;
using LedgerFlow.Application;
using LedgerFlow.Application.LedgerSummaries;
using LedgerFlow.Application.Transactions;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddLedgerSummariesModule(this IServiceCollection services)
    {
        var ledgerSummariesModule = typeof(LedgerSummariesModule).Namespace;
        services.AddHandlers(Assembly.GetExecutingAssembly(), ledgerSummariesModule);
    }
    public static void AddTransactionsModule(this IServiceCollection services)
    {
        var transactionsModule = typeof(TransactionsModule).Namespace;
        services.AddHandlers(Assembly.GetExecutingAssembly(), transactionsModule);
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
