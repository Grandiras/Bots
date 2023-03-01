using Discord;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserVoiceStateUpdatedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettingsStorage ServerSettings;
    private readonly ServerService ServerService;
    private readonly PrivateVoiceManager PrivateVoiceManager;


    public UserVoiceStateUpdatedService(DiscordSocketClient client, DiscordServerSettingsStorage serverSettings, ServerService serverService, PrivateVoiceManager privateVoiceManager)
    {
        Client = client;
        ServerSettings = serverSettings;
        ServerService = serverService;
        PrivateVoiceManager = privateVoiceManager;
    }


    public Task StartAsync()
    {
        Client.UserVoiceStateUpdated += OnUserVoiceStateUpdated;
        return Task.CompletedTask;
    }

    private async Task OnUserVoiceStateUpdated(SocketUser user, SocketVoiceState oldVoice, SocketVoiceState newVoice)
    {
        var oldServer = oldVoice.VoiceChannel is not null ? ServerSettings.ServerSettings[oldVoice.VoiceChannel.Guild.Id] : null;
        var newServer = newVoice.VoiceChannel is not null ? ServerSettings.ServerSettings[newVoice.VoiceChannel.Guild.Id] : null;

        if (oldVoice.VoiceChannel is not null and SocketVoiceChannel voiceChannel
            && voiceChannel.CategoryId == oldServer!.VoiceCategoryID
            && voiceChannel.ConnectedUsers.Count == 0)
            await CleanUpChannelAsync(voiceChannel);

        if (newVoice.VoiceChannel is not null && newVoice.VoiceChannel.Id == newServer!.NewTalkChannelID) await CreateNewVoiceAsync((SocketGuildUser)user);
        else if (newVoice.VoiceChannel is not null && newVoice.VoiceChannel.Id == newServer!.NewPrivateTalkChannelID) await CreateNewPrivateVoiceAsync((SocketGuildUser)user);
    }

    private async Task CleanUpChannelAsync(SocketVoiceChannel voiceChannel)
    {
        await voiceChannel.DeleteAsync();
        if (PrivateVoiceManager.GetPrivateChannelRoleAsync(voiceChannel) is not null and SocketRole role) await role.DeleteAsync();
    }

    private async Task CreateNewVoiceAsync(SocketGuildUser user)
    {
        var server = ServerService.GetServer(user.Guild.Id);
        var serverSettings = ServerSettings.ServerSettings[user.Guild.Id];

        var channel = await server.CreateVoiceChannelAsync("📺Talk", x => x.CategoryId = serverSettings.VoiceCategoryID);
        await MoveUserAsync(user, channel);
    }

    private async Task CreateNewPrivateVoiceAsync(SocketGuildUser user)
    {
        var server = ServerService.GetServer(user.Guild.Id);
        var serverSettings = ServerSettings.ServerSettings[user.Guild.Id];

        var role = await server.CreateRoleAsync("🔒Private Talk", isMentionable: false);
        var channel = await server.CreateVoiceChannelAsync("🔒Private Talk", x => x.CategoryId = serverSettings.VoiceCategoryID);
        await SetPrivateVoicePermissionsAsync(server, role, channel);

        await user.AddRoleAsync(role);
        await MoveUserAsync(user, channel);
    }
    private static async Task SetPrivateVoicePermissionsAsync(SocketGuild server, RestRole role, RestVoiceChannel channel)
    {
        await channel.AddPermissionOverwriteAsync(server.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));
    }

    private static async Task MoveUserAsync(SocketGuildUser user, RestVoiceChannel channel)
        => await user.ModifyAsync(x => x.Channel = channel);
}
