using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.AttachFiles)]
[Group("apply", "Apply for special positions with this command!")]
public sealed class ApplyCommand : InteractionModuleBase
{
	private readonly ServerService ServerService;
	private readonly ServerSettings ServerSettings;


	public ApplyCommand(ServerService serverService, ServerSettings serverSettings)
	{
		ServerService = serverService;
		ServerSettings = serverSettings;
	}


	[SlashCommand("moderator", "Apply for the role of a moderator!")]
	public async Task ModeratorAsync([Summary("file", "Attach your application here.")] IAttachment file)
	{
		if ((Context.User as SocketGuildUser)!.GuildPermissions.ManageMessages)
		{
			await RespondAsync("You are already a moderator!", ephemeral: true);
			return;
		}

		var embed = new EmbedBuilder()
			.WithTitle($"Application by {Context.User.Username}")
			.WithColor(Color.DarkGreen)
			.AddField(new EmbedFieldBuilder()
				.WithName("Attached application")
				.WithValue(file.Url));

		_ = await ServerService.GetServer(Context.Guild.Id).PublicUpdatesChannel.SendMessageAsync(embed: embed.Build());
		await RespondAsync("Your application has successfully been redirected to an employer.", ephemeral: true);
	}
}
