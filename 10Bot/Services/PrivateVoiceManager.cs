using Discord;
using Discord.WebSocket;

namespace TenBot.Services;
public class PrivateVoiceManager : IService
{
    private readonly ServerService ServerService;


    public PrivateVoiceManager(ServerService serverService) => ServerService = serverService;


    public SocketRole? GetPrivateChannelRoleAsync(SocketVoiceChannel voiceChannel)
    {
        var permission = voiceChannel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

        return permission is IEnumerable<Overwrite> overwrites and not null && overwrites.Any()
            ? ServerService.GetServer(voiceChannel.Guild.Id).Roles.First(x => x.Id == overwrites.First().TargetId)
            : null;
    }
}
