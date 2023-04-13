using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TenBot;
using TenBot.ClientEventServices;
using TenBot.Services;

var client = new DiscordSocketClient();

var settings = new SettingsService()
{
    RootDirectory = Directory.GetCurrentDirectory() + "/Data/",
    IsBeta = true,
    VersionNumber = 1
};

var host = Host
.CreateDefaultBuilder()
.ConfigureServices(services => _ = services
    .AddSingleton(client)
    .AddSingleton(settings)
    .AddSingleton<DiscordServerSettingsStorage>()

    .AddSingleton<InteractionService>()
    .AddSingleton<InteractionHandler>()

    .Scan(scan => scan
        .FromCallingAssembly()
        .AddClasses(classes => classes.AssignableTo<IClientEventService>())
        .As<IClientEventService>()
        .WithSingletonLifetime())

    .Scan(scan => scan
        .FromCallingAssembly()
        .AddClasses(classes => classes.AssignableTo<IService>())
        .AsSelf()
        .WithSingletonLifetime()))
.Build();

client.Log += async msg =>
{
    Console.WriteLine(msg.ToString());
    await Task.CompletedTask;
};

await host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();

host.Services
    .GetServices(typeof(IClientEventService))
    .ToList()
	.ForEach(async service => await ((IClientEventService)service!).StartAsync());

await client.LoginAsync(TokenType.Bot, host.Services.GetRequiredService<DiscordServerSettingsStorage>().ServerSettings.First().Value.Token);
await client.StartAsync();

await host.RunAsync();