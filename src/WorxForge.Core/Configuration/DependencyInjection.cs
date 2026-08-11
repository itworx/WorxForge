using Microsoft.Extensions.DependencyInjection;

namespace WorxForge.Core.Configuration;

public static class DependencyInjection
{
    public static IServiceCollection AddWorxForgeCore(this IServiceCollection services)
    {
        RegisterByInterface(services, typeof(DependencyInjection).Assembly, "Handler");
        RegisterByInterface(services, typeof(DependencyInjection).Assembly, "Validator");
        return services;
    }

    private static void RegisterByInterface(IServiceCollection services, System.Reflection.Assembly assembly, string suffix)
    {
        var types = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                type.Name.EndsWith(suffix, StringComparison.Ordinal));

        foreach (var type in types)
        {
            foreach (var implementedInterface in type.GetInterfaces())
            {
                services.AddTransient(implementedInterface, type);
            }
        }
    }
}
