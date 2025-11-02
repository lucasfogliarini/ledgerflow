using LedgerFlow;
using LedgerFlow.Application;
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

    private static IServiceCollection AddHandlers(this IServiceCollection services, Assembly assembly)
    {
        var handlerInterfaces = new[]
        {
            typeof(ICommandHandler<>),
            typeof(ICommandHandler<,>),
            typeof(IQueryHandler<,>),
            typeof(IDomainEventHandler<>)
        };

        var types = assembly.GetTypes();

        foreach (var type in types)
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
