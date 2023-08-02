using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserVoiceStateUpdatedService : IClientEventService
{
	private readonly DiscordSocketClient Client;
	private readonly ServerSettings ServerSettings;


	public UserVoiceStateUpdatedService(DiscordSocketClient client, ServerSettings serverSettings)
	{
		Client = client;
		ServerSettings = serverSettings;
	}


	public Task StartAsync()
	{
		Client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
		return Task.CompletedTask;
	}

	private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
	{
		if (oldVoice.VoiceChannel is not null and SocketVoiceChannel voiceChannel
			&& voiceChannel.CategoryId == ServerSettings.Configurations[oldVoice.VoiceChannel.Guild.Id].VoiceCategoryID && voiceChannel.ConnectedUsers.Count == 0)
			await CleanUpChannelAsync(voiceChannel);

		if (!((oldVoice.VoiceChannel is null || newVoice.VoiceChannel is null) && (oldVoice.VoiceChannel is null || newVoice.VoiceChannel is null || oldVoice.VoiceChannel!.Id != newVoice.VoiceChannel!.Id))) return;

		if (newVoice.VoiceChannel is not null)
		{
			if (newVoice.VoiceChannel.Id == ServerSettings.Configurations[newVoice.VoiceChannel.Guild.Id].NewTalkChannelID)
				await CreateNewVoiceAsync((SocketGuildUser)user);
			else if (newVoice.VoiceChannel.Id == ServerSettings.Configurations[newVoice.VoiceChannel.Guild.Id].NewPrivateTalkChannelID)
				await CreateNewPrivateVoiceAsync((SocketGuildUser)user);
		}
	}

	private async Task CleanUpChannelAsync(SocketVoiceChannel voiceChannel)
	{
		await voiceChannel.DeleteAsync();

		if (GetPrivateChannelRoleAsync(voiceChannel) is not null and SocketRole role) await role.DeleteAsync();
	}

	private async Task CreateNewVoiceAsync(SocketGuildUser user)
	{
		var channel = await Client.GetGuild(user.Guild.Id).CreateVoiceChannelAsync("📺Talk", x => x.CategoryId = ServerSettings.Configurations[user.Guild.Id].VoiceCategoryID);
		await MoveUserAsync(user, channel);
	}

	private async Task CreateNewPrivateVoiceAsync(SocketGuildUser user)
	{
		var guild = Client.GetGuild(user.Guild.Id);

		var role = await guild.CreateRoleAsync("🔒Private Talk", isMentionable: false);
		var channel = await guild.CreateVoiceChannelAsync("🔒Private Talk", x => x.CategoryId = ServerSettings.Configurations[user.Guild.Id].VoiceCategoryID);

		await SetPrivateVoicePermissionsAsync(guild, role, channel);

		await (user as IGuildUser)!.AddRoleAsync(role);
		await MoveUserAsync(user, channel);
	}
	private static async Task SetPrivateVoicePermissionsAsync(SocketGuild server, RestRole role, RestVoiceChannel channel)
	{
		await channel.AddPermissionOverwriteAsync(server.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
		await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));
	}

	private static async Task MoveUserAsync(SocketUser user, RestVoiceChannel channel)
		=> await (user as IGuildUser)!.ModifyAsync(x => x.Channel = channel);

	private SocketRole? GetPrivateChannelRoleAsync(SocketVoiceChannel voiceChannel)
	{
		var permission = voiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

		return permission is IEnumerable<Overwrite> overwrites && overwrites.Any()
			? Client.GetGuild(voiceChannel.Guild.Id).Roles.First(x => x.Id == overwrites.First().TargetId && !x.Name.EndsWith(" - Project") && !x.Name.EndsWith(" - Project - Public"))
			: null;
	}
}
