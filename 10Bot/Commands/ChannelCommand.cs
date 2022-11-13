using Discord;
using Discord.Interactions;
using TenBot.Models;

namespace TenBot.Commands;
[Group("channel", "A command to manage your current channel.")]
public sealed class ChannelCommand : InteractionModuleBase
{
    private readonly DiscordServerSettings ServerSettings;


    public ChannelCommand(DiscordServerSettings serverSettings) => ServerSettings = serverSettings;


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
}
