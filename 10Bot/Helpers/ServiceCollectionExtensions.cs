using Microsoft.Extensions.DependencyInjection;

namespace TenBot.Helpers;
internal static class ServiceCollectionExtensions
{
    public static void RegisterImplicitServices(this IServiceCollection collection, Type interfaceType, Type activatorType)
    {
        // Get all types in the executing assembly. There are many ways to do this, but this is fastest.
        foreach (var type in typeof(Program).Assembly.GetTypes())
        {
            if (interfaceType.IsAssignableFrom(type) && !type.IsAbstract)
                _ = collection.AddTransient(interfaceType, type);
        }

        // Register the activator so you can activate the instances.
        _ = collection.AddSingleton(activatorType);
    }
}
