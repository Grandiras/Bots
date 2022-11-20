using Discord.WebSocket;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ServerService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;

    public SocketGuild Server => Client.GetGuild(ServerSettings.GuildID);


    public ServerService(DiscordSocketClient client, DiscordServerSettings serverSettings)
    {
        Client = client;
        ServerSettings = serverSettings;
    }


    public SocketCategoryChannel GetCategory(Func<SocketCategoryChannel, bool> predicated) => Server.CategoryChannels.First(predicated);
    public SocketRole GetRole(Func<SocketRole, bool> predicate) => Server.Roles.First(predicate);

    public SocketCategoryChannel GetCategoryByRole(SocketRole role) 
        => Server.CategoryChannels.First(x => x.PermissionOverwrites.Any(x => role.Id == x.TargetId));

    public IEnumerable<SocketCategoryChannel> GetCategoriesByRoles(IEnumerable<SocketRole> roles)
        => Server.CategoryChannels.Where(x => x.PermissionOverwrites.Any(x => roles.Any(y => y.Id == x.TargetId)));
    public IEnumerable<SocketRole> GetRoles(Func<SocketRole, bool> predicate) => Server.Roles.Where(predicate);
}
