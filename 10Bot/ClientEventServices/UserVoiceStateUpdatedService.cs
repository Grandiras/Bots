using Discord;
using Discord.Rest;
using Discord.WebSocket;

namespace TenBot.ClientEventServices;
internal class UserVoiceStateUpdatedService : IClientEventService
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
        //// User enters new-private-talk channel
        //else if (newVoice.VoiceChannel.Id == Server.Settings.NewPrivateTalkChannelID)
        //{
        //    var channel = await CreateNewPrivateVoiceAsync((IGuildUser)user);
        //    await MoveUserAsync((IGuildUser)user, channel);
        //}

        // moven

        if (oldVoice.VoiceChannel is not null && oldVoice.VoiceChannel.CategoryId == ServerSettings.VoiceCategoryID && oldVoice.VoiceChannel.ConnectedUsers.Count == 0)
        {
            await oldVoice.VoiceChannel.DeleteAsync();
            return;
        }

        if (newVoice.VoiceChannel is null) return;

        if (newVoice.VoiceChannel!.Id == ServerSettings.NewTalkChannelID)
        {
            var channel = await CreateNewVoiceAsync();
            await MoveUserAsync(user, channel);
        }
    }

    private async Task<RestVoiceChannel> CreateNewVoiceAsync() 
        => await Client.GetGuild(ServerSettings.GuildID).CreateVoiceChannelAsync("Voice", x => x.CategoryId = ServerSettings.VoiceCategoryID);

    //private async Task<SocketVoiceChannel> CreateNewPrivateVoiceAsync(IGuildUser user)
    //{
    //    var role = await Server.Internal!.CreateRoleAsync("Private Voice", isMentionable: false);

    //    var channel = await Server.Internal.CreateVoiceChannelAsync("Private Voice", x => x.CategoryId = Server.Settings.VoiceCategoryID);
    //    await channel.AddPermissionOverwriteAsync(Server.Internal.EveryoneRole, new OverwritePermissions(
    //        viewChannel: PermValue.Deny));
    //    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
    //        viewChannel: PermValue.Allow));

    //    Server.VoiceChannels.Add(new PrivateVoiceSettings(channel, role, user));

    //    await user.AddRoleAsync(role);

    //    return channel;
    //}
    private static async Task MoveUserAsync(SocketUser user, RestVoiceChannel channel) 
        => await (user as IGuildUser)!.ModifyAsync(x => x.Channel = channel);
}
