﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using TenBot.ClientEventServices;
using TenBot.Helpers;

namespace TenBot;
public sealed record DiscordBotSettings(string Language);
public sealed record DiscordServerSettings(string Token,
                                           ulong GuildID,
                                           ulong NewTalkChannelID,
                                           ulong NewPrivateTalkChannelID,
                                           ulong VoiceCategoryID,
                                           ulong ModeratorRoleID,
                                           ulong MemberRoleID);

internal sealed class DiscordBot
{
    public static DiscordBot? Instance { get; private set; }

    public IHost Host { get; }


    public DiscordBot()
    {
        Instance = this;

        var config = new DiscordSocketConfig()
        {
        };

        var serviceConfig = new InteractionServiceConfig()
        {  
        };

        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/settings.json");
        var settings = JsonConvert.DeserializeObject<DiscordBotSettings>(json)!;

        var configJson = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/config.json");
        var serverSettings = JsonConvert.DeserializeObject<Dictionary<string, DiscordServerSettings>>(configJson)!["Selbsthilfegruppe_reloaded"];

        Host = Microsoft.Extensions.Hosting.Host
        .CreateDefaultBuilder()
        .UseContentRoot(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data")
        .ConfigureServices((context, services) =>
        {
            _ = services.AddSingleton(config);
            _ = services.AddSingleton<DiscordSocketClient>();

            _ = services.AddSingleton(serviceConfig);
            _ = services.AddSingleton<InteractionService>();
            _ = services.AddSingleton<InteractionHandler>();

            _ = services.AddSingleton(settings);
            _ = services.AddSingleton(serverSettings);

            _ = services.AddSingleton<ClientEventServiceActivator>();
            services.RegisterImplicitServices(typeof(IClientEventService), typeof(ClientEventServiceActivator));
        })
        .Build();
    }


    public async Task RunAsync()
    {
        var client = Host.Services.GetRequiredService<DiscordSocketClient>();
        var serverSettings = Host.Services.GetRequiredService<DiscordServerSettings>();

        client.Log += async (msg) =>
        {
            await Task.CompletedTask;
            Console.WriteLine(msg.ToString());
        };

        client.Ready += Ready;

        await Host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
        await Host.Services.GetRequiredService<ClientEventServiceActivator>().ActivateAsync();

        await client.LoginAsync(TokenType.Bot, serverSettings.Token);
        await client.StartAsync();

        await Task.Delay(-1);
    }

    private Task Ready() => Task.CompletedTask;
}
