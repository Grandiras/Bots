using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TenBot;
using TenBot.ClientEventServices;
using TenBot.Helpers;
using TenBot.Services;

var client = new DiscordSocketClient();

var settings = new SettingsService()
{
    RootDirectory = Directory.GetCurrentDirectory() + "/Data/",
    IsBeta = true
};

// configure the host
var host = Host
.CreateDefaultBuilder()
.ConfigureServices(services => _ = services
    .AddSingleton(client)
    .AddSingleton(settings)
    .AddSingleton<DiscordServerSettingsStorage>()

    .AddSingleton<InteractionService>()
    .AddSingleton<InteractionHandler>()

    .AddSingletonActivatorServices<IClientEventService, ClientEventServiceActivator>()
    .AddSingletonActivatorServices<IService, ServiceActivator>(false))
.Build();

// default logger for the Discord client
client.Log += async msg =>
{
    Console.WriteLine(msg.ToString());
    await Task.CompletedTask;
};

await host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
await host.Services.GetRequiredService<ClientEventServiceActivator>().ActivateAsync();

// connect to Discord
await client.LoginAsync(TokenType.Bot, host.Services.GetRequiredService<DiscordServerSettingsStorage>().ServerSettings.First().Value.Token);
await client.StartAsync();

// infinite timeout
await Task.Delay(-1);