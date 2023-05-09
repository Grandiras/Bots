using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class AcceptCommand : InteractionModuleBase
{
	private readonly ServerSettings ServerSettings;


	public AcceptCommand(ServerSettings serverSettings) => ServerSettings = serverSettings;


    [SlashCommand("accept", "Accept the rules to get the 'Member' role!")]
    public async Task AcceptAsync()
    {
        var server = ServerSettings.Configurations[Context.Guild.Id];

        if (Context.User is not SocketGuildUser user)
        {
            await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
            return;
        }

        if (user.Roles.Any(x => x.Id == server.MemberRoleID))
        {
            await RespondAsync("Really? You already have this role -_-", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(server.MemberRoleID);
        await RespondAsync("You got that role. Try /discover to find projects to join!", ephemeral: true);
    }
}
