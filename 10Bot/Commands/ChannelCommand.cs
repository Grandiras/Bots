using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Helpers;
using TenBot.Models;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.Connect)]
[Group("channel", "A command to manage your current channel.")]
public sealed class ChannelCommand : InteractionModuleBase
{
    private readonly DiscordServerSettings ServerSettings;
    private readonly DiscordSocketClient Client;


    public ChannelCommand(DiscordServerSettings serverSettings, DiscordSocketClient client)
    {
        ServerSettings = serverSettings;
        Client = client;
    }


    [SlashCommand("rename", "Allows you to rename your current channel, even if you aren't allowed to through your permissions!")]
    public async Task RenameAsync([Summary("new_name", "Enter a new name for the channel.")] string newName)
    {
        var voiceChannel = (Context.User as IGuildUser)!.VoiceChannel;
        if (voiceChannel is null)
        {
            await RespondAsync($"You have to be in a voice in order to rename it!", ephemeral: true);
            return;
        }
        if (voiceChannel!.CategoryId != ServerSettings.VoiceCategoryID)
        {
            await RespondAsync($"You can only rename generated voice channels!", ephemeral: true);
            return;
        }

        await voiceChannel.ModifyAsync(x => x.Name = newName);
        await RespondAsync($"The name of your current channel was set to '{newName}'.", ephemeral: true);
    }

    [UserCommand("Invite to talk")]
    [SlashCommand("invite", "Invites somebody to your private channel.")]
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
