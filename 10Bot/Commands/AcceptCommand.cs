using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class AcceptCommand : InteractionModuleBase
{
    private readonly DiscordServerSettingsStorage ServerSettings;


    public AcceptCommand(DiscordServerSettingsStorage serverSettings) => ServerSettings = serverSettings;


    [SlashCommand("accept", "Accept the rules to get the 'Member' role!")]
    public async Task AcceptAsync()
    {
        var serverSettings = ServerSettings.ServerSettings[Context.Guild.Id];

        if (Context.User is not SocketGuildUser user)
        {
            await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
            return;
        }

        if (user.Roles.Any(x => x.Id == serverSettings.MemberRoleID))
        {
            await RespondAsync("Really? You already have this role -_-", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(serverSettings.MemberRoleID);
        await RespondAsync("You got that role. Try /discover to find projects to join!", ephemeral: true);
    }
}
