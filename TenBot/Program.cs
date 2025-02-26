﻿using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TenBot.Configuration;
using TenBot.Features;
using TenBot.Helpers;
using TenBot.Services;
using TenBot.StandardFeatures;

var builder = new HostApplicationBuilder();

builder.AddServiceDefaults();

builder.Services
    .AddSingleton(new SecretsConfiguration(builder.Configuration))
    .AddSingleton(builder.Configuration.GetSection("Bot").Get<BotConfiguration>()!);

builder.Services
    .AddSingleton<DiscordSocketClient>()
    .AddSingleton(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent })
    .AddSingleton<ServerInteractionHandler>()
    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), x.GetRequiredService<InteractionServiceConfig>()))
    .AddSingleton(new InteractionServiceConfig() { UseCompiledLambda = true });

builder.Services.Scan(scan => scan.FromCallingAssembly()
    .AddClasses(classes => classes.AssignableTo<IService>())
        .AsSelf()
        .WithSingletonLifetime()
    .AddClasses(classes => classes.AssignableTo<IStandardFeature>())
        .AsSelf()
        .WithSingletonLifetime()
    .AddClasses(classes => classes.AssignableTo<IFeature>())
        .AsSelf()
        .WithSingletonLifetime());

var host = builder.Build();

var client = host.Services.GetRequiredService<DiscordSocketClient>();
var configurator = host.Services.GetRequiredService<BotConfigurator>();
var logger = host.Services.GetRequiredService<ILogger<Program>>();

client.Log += async (msg) =>
{
    logger.Log(msg.Severity.ToLogLevel(), "{}", msg.ToString());
    await Task.CompletedTask;
};
client.Ready += async () =>
{
    foreach (var service in host.Services.GetAllServicesWith<IMustPostInitialize>()) await service.PostInitializeAsync();
};

foreach (var service in host.Services.GetAllServicesWith<IMustInitialize>()) _ = service.InitializeAsync();

await client.LoginAsync(TokenType.Bot, configurator.GetToken());
await client.StartAsync();

await client.SetCustomStatusAsync("Use /help for more information!");

await Task.Delay(-1); // Infinite timeout