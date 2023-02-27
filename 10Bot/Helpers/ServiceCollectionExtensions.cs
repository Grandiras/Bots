using Microsoft.Extensions.DependencyInjection;

namespace TenBot.Helpers;
internal static class ServiceCollectionExtensions
{
    public static IServiceCollection AddTransientActivatorServices<TInterface, TActivator>(this IServiceCollection collection, bool includeInterface = true)
    {
        // Get all types in the executing assembly. There are many ways to do this, but this is fastest.
        foreach (var type in typeof(Program).Assembly.GetTypes())
        {
            if (typeof(TInterface).IsAssignableFrom(type) && !type.IsAbstract)
                _ = includeInterface ? collection.AddTransient(typeof(TInterface), type) : collection.AddTransient(type);
        }

        // Register the activator so you can activate the instances.
        _ = collection.AddSingleton(typeof(TActivator));

        return collection;
    }
    
    public static IServiceCollection AddSingletonActivatorServices<TInterface, TActivator>(this IServiceCollection collection, bool includeInterface = true)
    {
        // Get all types in the executing assembly. There are many ways to do this, but this is fastest.
        foreach (var type in typeof(Program).Assembly.GetTypes())
        {
            if (typeof(TInterface).IsAssignableFrom(type) && !type.IsAbstract)
                _ = includeInterface ? collection.AddSingleton(typeof(TInterface), type) : collection.AddSingleton(type);
        }

        // Register the activator so you can activate the instances.
        _ = collection.AddSingleton(typeof(TActivator));

        return collection;
    }
}
