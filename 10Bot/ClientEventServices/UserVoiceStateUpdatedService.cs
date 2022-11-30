using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Helpers;
using TenBot.Models;

namespace TenBot.ClientEventServices;
internal sealed class UserVoiceStateUpdatedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;


    public UserVoiceStateUpdatedService(DiscordSocketClient client, DiscordServerSettings serverSettings)
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

        if (oldVoice.VoiceChannel is not null and SocketVoiceChannel voiceChannel
            && voiceChannel.CategoryId == ServerSettings.VoiceCategoryID
            && voiceChannel.ConnectedUsers.Count == 0)
            await CleanUpChannelAsync(voiceChannel);
        else if (newVoice.VoiceChannel?.Id == ServerSettings.NewTalkChannelID) await CreateNewVoiceAsync(user);
        else if (newVoice.VoiceChannel?.Id == ServerSettings.NewPrivateTalkChannelID) await CreateNewPrivateVoiceAsync(user);
    }

    private async Task CleanUpChannelAsync(SocketVoiceChannel voiceChannel)
    {
        await voiceChannel.DeleteAsync();

        var role = PrivateVoiceManager.GetPrivateChannelRoleAsync(voiceChannel, ServerSettings, Client);
        if (role is not null) await role.DeleteAsync();
    }

    private async Task CreateNewVoiceAsync(SocketUser user)
    {
        var channel = await Client.GetGuild(ServerSettings.GuildID).CreateVoiceChannelAsync("📺Voice", x => x.CategoryId = ServerSettings.VoiceCategoryID);
        await MoveUserAsync(user, channel);
    }

    private async Task CreateNewPrivateVoiceAsync(SocketUser user)
    {
        var server = Client.GetGuild(ServerSettings.GuildID);

        var role = await server.CreateRoleAsync("🔒Private Voice", isMentionable: false);
        var channel = await server.CreateVoiceChannelAsync("🔒Private Voice", x => x.CategoryId = ServerSettings.VoiceCategoryID);

        await SetPrivateVoicePermissionsAsync(server, role, channel);

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
