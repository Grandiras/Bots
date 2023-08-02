using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using TenBot.Services;

namespace TenBot;
/// <summary>
/// This is a preset from Discord.NET to initialize all interaction contexts.
/// </summary>
public sealed class InteractionHandler
{
	private readonly DiscordSocketClient Client;
	private readonly InteractionService Handler;
	private readonly IServiceProvider Services;
	private readonly ServerSettings ServerSettings;


	public InteractionHandler(DiscordSocketClient client, InteractionService handler, IServiceProvider services, ServerSettings serverSettings)
	{
		Client = client;
		Handler = handler;
		Services = services;
		ServerSettings = serverSettings;
	}


	public async Task InitializeAsync()
	{
		Client.Ready += ReadyAsync;
		Client.JoinedGuild += GuildAddedAsync;
		Handler.Log += LogAsync;

		_ = await Handler.AddModulesAsync(Assembly.GetEntryAssembly(), Services); // register all existing interaction modules from the current assembly

		Client.InteractionCreated += HandleInteraction;
	}


	private Task LogAsync(LogMessage log)
	{
		Console.WriteLine(log);
		return Task.CompletedTask;
	}

	private async Task ReadyAsync()
	{
		foreach (var serverID in ServerSettings.Configurations.Keys) _ = await Handler.RegisterCommandsToGuildAsync(serverID, true);
	}
	private async Task GuildAddedAsync(SocketGuild server) => _ = await Handler.RegisterCommandsToGuildAsync(server.Id, true);

	private async Task HandleInteraction(SocketInteraction interaction)
	{
		try
		{
			var context = new SocketInteractionContext(Client, interaction); // glue for the commands and their corresponding services
			var result = await Handler.ExecuteCommandAsync(context, Services);
		}
		catch
		{
			// If Slash Command execution fails it is most likely that the original interaction acknowledgement will persist. It is a good idea to delete the original
			// response, or at least let the user know that something went wrong during the command execution.
			if (interaction.Type is InteractionType.ApplicationCommand) _ = await interaction.GetOriginalResponseAsync().ContinueWith(async (msg) => await msg.Result.DeleteAsync());
		}
	}
}