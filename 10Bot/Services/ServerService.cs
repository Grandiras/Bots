using Discord.WebSocket;

namespace TenBot.Services;
public sealed class ServerService : IService
{
	private readonly DiscordSocketClient Client;


	public ServerService(DiscordSocketClient client) => Client = client;


	public SocketGuild GetServer(ulong guildID) => Client.GetGuild(guildID);

	public SocketRole GetRole(Func<SocketRole, bool> predicate, ulong guildID) => Client.GetGuild(guildID).Roles.First(predicate);

	public SocketCategoryChannel GetCategoryByRole(SocketRole role, ulong guildID)
		=> Client.GetGuild(guildID).CategoryChannels.First(x => x.PermissionOverwrites.Any(x => role.Id == x.TargetId));

	public IEnumerable<SocketCategoryChannel> GetCategoriesByRoles(IEnumerable<SocketRole> roles, ulong guildID)
		=> Client.GetGuild(guildID).CategoryChannels.Where(x => x.PermissionOverwrites.Any(x => roles.Any(y => y.Id == x.TargetId)));
	public IEnumerable<SocketRole> GetRoles(Func<SocketRole, bool> predicate, ulong guildID) => Client.GetGuild(guildID).Roles.Where(predicate);
}
