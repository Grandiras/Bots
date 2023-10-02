using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using System.Reflection;
using TenBot.Helpers;
using TenBot.ServerAbstractions;

namespace TenBot.Services;
public sealed class ServerInteractionHandler : IService, IMustInitialize, IMustPostInitialize
{
    private readonly DiscordSocketClient Client;
    private readonly InteractionService Interactions;
    private readonly IServiceProvider Services;
    private readonly ILogger<ServerInteractionHandler> Logger;
    private readonly ServerService ServerManager;
    private readonly FeatureService FeatureManager;

    public ServerInteractionHandler(DiscordSocketClient client, InteractionService interactions, IServiceProvider services, ILogger<ServerInteractionHandler> logger, ServerService serverManager, FeatureService featureManager)
    {
        Client = client;
        Interactions = interactions;
        Services = services;
        Logger = logger;
        ServerManager = serverManager;
        FeatureManager = featureManager;
    }


    public async Task InitializeAsync()
    {
        Client.JoinedGuild += GuildAddedAsync;
        Client.LeftGuild += GuildLeftAsync;

        Interactions.Log += LogAsync;

        _ = await Interactions.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

        Client.InteractionCreated += HandleInteractionAsync;
    }
    public async Task PostInitializeAsync() { foreach (var server in ServerManager.Servers) { _ = await Interactions.AddModulesToGuildAsync(server.Id, true, FeatureManager.GetFeatureModuleInfosForServer(server).ToArray()); } }

    private Task LogAsync(LogMessage log)
    {
        Logger.Log(log.Severity.ToLogLevel(), "{}", log.ToString());
        return Task.CompletedTask;
    }

    private async Task GuildAddedAsync(SocketGuild server)
    {
        await ServerManager.AddServerAsync(server);
        _ = await Interactions.AddModulesToGuildAsync(server.Id, true, FeatureManager.GetFeatureModuleInfosForServer(ServerManager.GetServerById(server.Id).AsT0).ToArray());

        Logger.LogInformation("Joined server: {}", server.Name);
    }
    private async Task GuildLeftAsync(SocketGuild server)
    {
        await ServerManager.RemoveServerAsync(server);

        Logger.LogInformation("Left server: {}", server.Name);
    }

    private async Task HandleInteractionAsync(SocketInteraction interaction)
    {
        try { _ = await Interactions.ExecuteCommandAsync(new ServerInteractionContext(Client, interaction, ServerManager, FeatureManager), Services); }
        catch
        {
            // TODO proper error handling and reporting / feedback
            if (interaction.Type is InteractionType.ApplicationCommand) _ = await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}
