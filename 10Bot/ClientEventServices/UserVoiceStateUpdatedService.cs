using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Helpers;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserVoiceStateUpdatedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettingsStorage ServerSettings;


    public UserVoiceStateUpdatedService(DiscordSocketClient client, DiscordServerSettingsStorage serverSettings)
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
        // TODO moving

        var server = ServerSettings.Settings[(user as SocketGuildUser)!.Guild.Id];

        if (oldVoice.VoiceChannel is not null and SocketVoiceChannel voiceChannel
            && voiceChannel.CategoryId == server.VoiceCategoryID
            && voiceChannel.ConnectedUsers.Count == 0)
            await CleanUpChannelAsync(voiceChannel);
        else if (newVoice.VoiceChannel?.Id == server.NewTalkChannelID) await CreateNewVoiceAsync((user as SocketGuildUser)!);
        else if (newVoice.VoiceChannel?.Id == server.NewPrivateTalkChannelID) await CreateNewPrivateVoiceAsync((user as SocketGuildUser)!);
    }

    private async Task CleanUpChannelAsync(SocketVoiceChannel voiceChannel)
    {
        await voiceChannel.DeleteAsync();

        var role = PrivateVoiceManager.GetPrivateChannelRoleAsync(voiceChannel, ServerSettings.Settings[voiceChannel.Guild.Id], Client);
        if (role is not null) await role.DeleteAsync();
    }

    private async Task CreateNewVoiceAsync(SocketGuildUser user)
    {
        var server = ServerSettings.Settings[user.Guild.Id];
        var channel = await Client.GetGuild(server.GuildID).CreateVoiceChannelAsync("📺Talk", x => x.CategoryId = server.VoiceCategoryID);
        await MoveUserAsync(user, channel);
    }

    private async Task CreateNewPrivateVoiceAsync(SocketGuildUser user)
    {
        var server = ServerSettings.Settings[user.Guild.Id];
        var guild = Client.GetGuild(server.GuildID);

        var role = await guild.CreateRoleAsync("🔒Private Talk", isMentionable: false);
        var channel = await guild.CreateVoiceChannelAsync("🔒Private Talk", x => x.CategoryId = server.VoiceCategoryID);

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
}
