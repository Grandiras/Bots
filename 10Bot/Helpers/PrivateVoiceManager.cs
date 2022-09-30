using Discord;
using Discord.WebSocket;

namespace TenBot.Helpers;
internal static class PrivateVoiceManager
{
    public static SocketRole? GetPrivateChannelRoleAsync(IGuildUser user, DiscordServerSettings serverSettings, DiscordSocketClient client)
    {
        if (user.VoiceChannel is null) return null;

        var permissions = user.VoiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

        return permissions is IEnumerable<Overwrite> overwrites and not null && overwrites.Any()
            ? client.GetGuild(serverSettings.GuildID).Roles
                    .First(x => x.Id == overwrites.First().TargetId)
            : null;
    }

    public static SocketRole? GetPrivateChannelRoleAsync(SocketVoiceChannel voiceChannel, DiscordServerSettings serverSettings, DiscordSocketClient client)
    {
        var permission = voiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

        return permission is IEnumerable<Overwrite> overwrites && overwrites.Any()
            ? client.GetGuild(serverSettings.GuildID).Roles
                  .First(x => x.Id == overwrites.First().TargetId)
            : null;
    }
}
