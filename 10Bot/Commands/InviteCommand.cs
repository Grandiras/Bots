using Discord.Interactions;
using Discord;
using TenBot.Helpers;
using Discord.WebSocket;
using TenBot.Models;

namespace TenBot.Commands;
public sealed class InviteCommand : InteractionModuleBase
{
    private readonly DiscordServerSettings ServerSettings;
    private readonly DiscordSocketClient Client;


    public InviteCommand(DiscordServerSettings serverSettings, DiscordSocketClient client)
    {
        ServerSettings = serverSettings;
        Client = client;
    }


    [SlashCommand("invite", "Invite someone to your private channel!")]
    public async Task InviteAsync(IGuildUser user)
    {
        var role = PrivateVoiceManager.GetPrivateChannelRoleAsync((IGuildUser)Context.User, ServerSettings, Client);

        if (role is null)
        {
            await RespondAsync($"You can't invite someone as you are not in any private voice channel!", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(role.Id);
        await RespondAsync($"{user.Mention} was added to this channel.", ephemeral: true);
    }
}
