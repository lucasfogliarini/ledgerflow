using Microsoft.Extensions.Hosting;
using Wolverine;

namespace Microsoft.Extensions.DependencyInjection;

public static class DependencyInjection
{
    public static void AddTransactionsModule(this IHostApplicationBuilder builder)
    {
        builder.UseWolverineFx();
    }

    private static void UseWolverineFx(this IHostApplicationBuilder builder)
    {
        builder.UseWolverine(opts =>
        {
            opts.Durability.Mode = DurabilityMode.MediatorOnly;
        });
    }
}
