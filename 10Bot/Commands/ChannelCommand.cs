using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Helpers;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.Connect)]
[Group("channel", "A command to manage your current channel.")]
public sealed class ChannelCommand : InteractionModuleBase
{
    private readonly DiscordServerSettingsStorage ServerSettings;
    private readonly DiscordSocketClient Client;
    private readonly ServerService ServerService;
    private readonly PrivateVoiceManager PrivateVoiceManager;


    public ChannelCommand(DiscordServerSettingsStorage serverSettings, DiscordSocketClient client, ServerService serverService, PrivateVoiceManager privateVoiceManager)
    {
        ServerSettings = serverSettings;
        Client = client;
        ServerService = serverService;
        PrivateVoiceManager = privateVoiceManager;
    }


    [SlashCommand("rename", "Allows you to rename your current channel, even if you aren't allowed to through your permissions!")]
    public async Task RenameAsync([Summary("new_name", "Enter a new name for the channel.")] string newName)
    {
        var serverSettings = ServerSettings.Settings[Context.Guild.Id];

        var voiceChannel = ((SocketGuildUser)Context.User).VoiceChannel;
        if (voiceChannel is null)
        {
            await RespondAsync($"You have to be in a voice in order to rename it!", ephemeral: true);
            return;
        }
        if (voiceChannel!.CategoryId != serverSettings.VoiceCategoryID)
        {
            await RespondAsync($"You can only rename generated voice channels!", ephemeral: true);
            return;
        }

        await voiceChannel.ModifyAsync(x => x.Name = newName);
        await RespondAsync($"The name of your current channel was set to '{newName}'.", ephemeral: true);
    }

    [UserCommand("Invite to talk")]
    [SlashCommand("invite", "Invites somebody to your private channel.")]
    public async Task InviteAsync(SocketGuildUser user)
    {
        var role = PrivateVoiceManager.GetPrivateChannelRoleAsync(((SocketGuildUser)Context.User).VoiceChannel);

        if (role is null)
        {
            await RespondAsync($"You can't invite someone as you are not in any private voice channel!", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(role);
        await RespondAsync($"{user.Mention} was added to this channel.", ephemeral: true);
    }

    //[SlashCommand("invite-all", "Invites all members of a specific project.")]
    public async Task InviteAllAsync([Summary("name", "The project's name."),
                                      Autocomplete(typeof(UserProjectAutoCompleteHandler))] string name)
    {
        var role = PrivateVoiceManager.GetPrivateChannelRoleAsync(((SocketGuildUser)Context.User).VoiceChannel);

        if (role is null)
        {
            await RespondAsync($"You can't invite someone as you are not in any private voice channel!", ephemeral: true);
            return;
        }

        var projectRole = ServerService.GetRole(x => x.Name.Split(" -")[0] == name, Context.Guild.Id);

        foreach (var user in projectRole.Members.Where(x => !x.Roles.Any(y => y == role))) 
            await user.AddRoleAsync(role);

        await RespondAsync($"The users were added to this channel.", ephemeral: true);
    }
}
