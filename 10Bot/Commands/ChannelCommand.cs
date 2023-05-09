using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.Connect)]
[Group("channel", "A command to manage your current channel.")]
public sealed class ChannelCommand : InteractionModuleBase
{
	private readonly ServerSettings ServerSettings;
	private readonly DiscordSocketClient Client;


	public ChannelCommand(ServerSettings serverSettings, DiscordSocketClient client)
	{
		ServerSettings = serverSettings;
		Client = client;
	}


	[SlashCommand("rename", "Allows you to rename your current channel, even if you aren't allowed to through your permissions!")]
	public async Task RenameAsync([Summary("new_name", "Enter a new name for the channel.")] string newName)
	{
		var server = ServerSettings.Configurations[Context.Guild.Id];

		var voiceChannel = (Context.User as IGuildUser)!.VoiceChannel;
		if (voiceChannel is null)
		{
			await RespondAsync($"You have to be in a voice in order to rename it!", ephemeral: true);
			return;
		}
		if (voiceChannel!.CategoryId != server.VoiceCategoryID)
		{
			await RespondAsync($"You can only rename generated voice channels!", ephemeral: true);
			return;
		}

		await voiceChannel.ModifyAsync(x => x.Name = newName);
		await RespondAsync($"The name of your current channel was set to '{newName}'.", ephemeral: true);
	}

	[UserCommand("Invite to talk")]
	[SlashCommand("invite", "Invites somebody to your private channel.")]
	public async Task InviteAsync(IGuildUser user)
	{
		_ = ServerSettings.Configurations[Context.Guild.Id];
		var role = GetPrivateChannelRoleAsync((IGuildUser)Context.User);

		if (role is null)
		{
			await RespondAsync($"You can't invite someone as you are not in any private voice channel!", ephemeral: true);
			return;
		}

		await user.AddRoleAsync(role.Id);
		await RespondAsync($"{user.Mention} was added to this channel.", ephemeral: true);
	}

	private SocketRole? GetPrivateChannelRoleAsync(IGuildUser user)
	{
		var permission = user.VoiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

		return permission is IEnumerable<Overwrite> overwrites && overwrites.Any()
			? Client.GetGuild(user.Guild.Id).Roles.First(x => x.Id == overwrites.First().TargetId)
			: null;
	}
}
