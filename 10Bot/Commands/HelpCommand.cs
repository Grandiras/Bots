using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class HelpCommand : InteractionModuleBase
{
	private readonly ServerSettings ServerSettings;
	private readonly DiscordSocketClient Client;


	public HelpCommand(ServerSettings serverSettings, DiscordSocketClient client)
	{
		ServerSettings = serverSettings;
		Client = client;
	}


	[SlashCommand("help", "Displayes a list of all commands and their descriptions existing here.")]
	public async Task RenameAsync()
	{
		if (Context.User is not IGuildUser)
		{
			await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
			return;
		}

		var embed = new EmbedBuilder()
		{
			Title = "10Bot Commands",
			Color = Color.Gold,
		};

		foreach (var item in Client.GetGuild(Context.Guild.Id).GetApplicationCommandsAsync().Result)
			_ = embed.AddField(item.Name, item.Description);

		await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
	}
}
