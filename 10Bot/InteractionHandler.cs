using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;

namespace TenBot;

public class InteractionHandler
{
    private readonly DiscordSocketClient Client;
    private readonly InteractionService Handler;
    private readonly IServiceProvider Services;
    private readonly DiscordServerSettings ServerSettings;

    public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, DiscordServerSettings serverSettings)
    {
        Client = client;
        Handler = handler;
        Services = services;
        ServerSettings = serverSettings;
    }

    public async Task InitializeAsync()
    {
        // Process when the client is ready, so we can register our commands.
        Client.Ready += ReadyAsync;
        Handler.Log += LogAsync;

        // Add the public modules that inherit InteractionModuleBase<T> to the InteractionService
        _ = await Handler.AddModulesAsync(Assembly.GetEntryAssembly(), Services);

        // Process the InteractionCreated payloads to execute Interactions commands
        Client.InteractionCreated += HandleInteraction;
    }

    private Task LogAsync(LogMessage log)
    {
        Console.WriteLine(log);
        return Task.CompletedTask;
    }

    private async Task ReadyAsync() => _ = await Handler.RegisterCommandsToGuildAsync(ServerSettings.GuildID, true);

    private async Task HandleInteraction(SocketInteraction interaction)
    {
        try
        {
            // Create an execution context that matches the generic type parameter of your InteractionModuleBase<T> modules.
            var context = new SocketInteractionContext(Client, interaction);

            // Execute the incoming command.
            var result = await Handler.ExecuteCommandAsync(context, Services);

            if (!result.IsSuccess)
                switch (result.Error)
                {
                    case InteractionCommandError.UnmetPrecondition:
                        // implement
                        break;
                    default:
                        break;
                }
        }
        catch
        {
            // If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
            // response, or at least let the user know that something went wrong during the command execution.
            if (interaction.Type is InteractionType.ApplicationCommand)
                _ = await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
        }
    }
}