using Discord;
using Discord.Interactions;
using TenBot.Models;

namespace TenBot.Commands;
public sealed class AcceptCommand : InteractionModuleBase
{
    private readonly DiscordServerSettings ServerSettings;


    public AcceptCommand(DiscordServerSettings serverSettings) => ServerSettings = serverSettings;


    [SlashCommand("accept", "Accept the rules to get the 'Member' role!")]
    public async Task AcceptAsync()
    {
        if (Context.User is not IGuildUser user)
        {
            await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
            return;
        }

        if (user.RoleIds.Contains(ServerSettings.MemberRoleID))
        {
            await RespondAsync("Really? You already have this role -_-", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(ServerSettings.MemberRoleID);
        await RespondAsync("Thank you, have fun!", ephemeral: true);
    }
}
