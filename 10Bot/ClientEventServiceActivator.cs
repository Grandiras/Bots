using TenBot.ClientEventServices;

namespace TenBot;
internal sealed class ClientEventServiceActivator
{
    private readonly IEnumerable<IClientEventService> Services;


    public ClientEventServiceActivator(IEnumerable<IClientEventService> services) => Services = services;


    public async Task ActivateAsync()
    {
        foreach (var service in Services) await service.StartAsync();
    }
}
