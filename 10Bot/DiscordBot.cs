using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using TenBot.ClientEventServices;
using TenBot.Helpers;
using TenBot.Models;
using TenBot.Services;

namespace TenBot;
internal sealed class DiscordBot
{
    private readonly IHost Host;


    public DiscordBot()
    {
        // runtime config
        var config = new DiscordSocketConfig();
        var serviceConfig = new InteractionServiceConfig();

        // locally stored config (use beta-config for beta bot version)
        var configJson = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/config.json");
        var serverSettings = new DiscordServerSettingsStorage(JsonConvert.DeserializeObject<Dictionary<ulong, DiscordServerSettings>>(configJson)!);

        // configure the host
        Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .ConfigureLogging((logger) => logger.AddConsole()) // logging
        .ConfigureServices((context, services) =>
        {
            _ = services
                .AddSingleton(config)
                .AddSingleton<DiscordSocketClient>(); // connnection to Discord

            _ = services
                .AddSingleton(serviceConfig)
                .AddSingleton<InteractionService>()
                .AddSingleton<InteractionHandler>();

            _ = services
                .AddSingleton(serverSettings);

            // custom services
            _ = services
                .AddSingleton<WelcomeMessages>()
                .AddSingleton<CustomCommands>()
                .AddSingleton<ProjectTemplates>()
                .AddSingleton<QuotesService>()
                .AddSingleton<ServerService>()
                .AddSingleton<PollService>();

            // activator for all interaction event singletons
            _ = services.AddActivatorServices<IClientEventService, ClientEventServiceActivator>();
        })
        .Build();
    }


    public async Task RunAsync()
    {
        var client = Host.Services.GetRequiredService<DiscordSocketClient>();
        var serverSettings = Host.Services.GetRequiredService<DiscordServerSettingsStorage>();

        // default logger for the Discord client
        client.Log += async (msg) =>
        {
            Console.WriteLine(msg.ToString());
            await Task.CompletedTask;
        };

        await Host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await Host.Services.GetRequiredService<ClientEventServiceActivator>().ActivateAsync();

        // connect to Discord
        await client.LoginAsync(TokenType.Bot, serverSettings.Settings.First().Value.Token);
        await client.StartAsync();

        // infinite timeout
        await Task.Delay(-1);
    }
}
