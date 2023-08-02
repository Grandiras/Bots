using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Helpers;
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
		var role = GetPrivateChannelRoleAsync((IGuildUser)Context.User);

		if (role is null)
		{
			await RespondAsync($"You can't invite someone as you are not in any private voice channel!", ephemeral: true);
			return;
		}

		await user.AddRoleAsync(role.Id);
		await RespondAsync($"{user.Mention} was added to this channel.", ephemeral: true);
	}

	[SlashCommand("invite-project", "Invites an entire project to your channel.")]
	public async Task InviteProjectAsync([Summary("project", "The project to invite (you must be a member of this project!)."), Autocomplete(typeof(PersonalProjectAutoCompleteHandler))] string project)
	{
		await ((IGuildUser)Context.User).VoiceChannel.AddPermissionOverwriteAsync(Context.Guild.Roles.First(x => x.Name.StartsWith(project)), new OverwritePermissions(viewChannel: PermValue.Allow));
		await RespondAsync($"All '{project}' members were added to this channel.", ephemeral: true);
	}

	[SlashCommand("is-private", "Tells you, whether you current channel is public or private.")]
	public async Task IsPrivateAsync()
		=> await RespondAsync($"Your current channel is {(((IGuildUser)Context.User).VoiceChannel.PermissionOverwrites.Any(x => x.Permissions.ViewChannel == PermValue.Allow) ? "private" : "public")}", ephemeral: true);

	[SlashCommand("convert", "Converts your current voice channel between public and private.")]
	public async Task ConvertToAsync()
	{
		var user = (IGuildUser)Context.User;

		if (user.VoiceChannel.CategoryId != ServerSettings.Configurations[Context.Guild.Id].VoiceCategoryID)
		{
			await RespondAsync("You need to be in a generated voice channel to do this!", ephemeral: true);
			return;
		}

		if (user.VoiceChannel.PermissionOverwrites.Any(x => x.Permissions.ViewChannel == PermValue.Allow))
		{
			await GetPrivateChannelRoleAsync(user)!.DeleteAsync();
			user.VoiceChannel.PermissionOverwrites.Select(x => Context.Guild.GetRole(x.TargetId)).ToList().ForEach(y => user.VoiceChannel.RemovePermissionOverwriteAsync(y));
			return;
		}

		var role = await Context.Guild.CreateRoleAsync(user.VoiceChannel.Name, isMentionable: false);

		await user.VoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
		await user.VoiceChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));

		await user.VoiceChannel.GetUsersAsync().Flatten().ForEachAsync(x => x.AddRoleAsync(role));

		await RespondAsync("Successfully converted your current voice channel.", ephemeral: true);
	}

	private SocketRole? GetPrivateChannelRoleAsync(IGuildUser user)
	{
		var permission = user.VoiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

		return permission is IEnumerable<Overwrite> overwrites && overwrites.Any()
			? Client.GetGuild(user.VoiceChannel.Guild.Id).Roles.First(x => x.Id == overwrites.First().TargetId && !x.Name.EndsWith(" - Project") && !x.Name.EndsWith(" - Project - Public"))
			: null;
	}
}
