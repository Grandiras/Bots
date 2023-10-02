using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace TenBot.Helpers;
public static class IServiceProviderExtensions
{
    private static object? Site;


    public static List<ServiceDescriptor> GetServices(this IServiceProvider provider)
    {
        Site ??= typeof(ServiceProvider).GetProperty("CallSiteFactory", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(provider);
        var desc = Site!.GetType().GetField("_descriptors", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(Site) as ServiceDescriptor[];
        return desc!.ToList();
    }
    public static IEnumerable<T> GetAllServicesWith<T>(this IServiceProvider provider)
        => provider.GetServices().Where(x => x.ServiceType.GetInterfaces().Any(y => y == typeof(T))).Select(x => (T)provider.GetRequiredService(x.ServiceType));
}
