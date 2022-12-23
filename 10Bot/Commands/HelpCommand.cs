using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class HelpCommand : InteractionModuleBase
{
    private readonly DiscordServerSettingsStorage ServerSettings;
    private readonly DiscordSocketClient Client;


    public HelpCommand(DiscordServerSettingsStorage serverSettings, DiscordSocketClient client)
    {
        ServerSettings = serverSettings;
        Client = client;
    }


    [SlashCommand("help", "Displayes a list of all commands and their descriptions existing here.")]
    public async Task RenameAsync()
    {
        var server = ServerSettings.Settings[Context.Guild.Id];

        if (Context.User is not IGuildUser)
        {
            await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
            return;
        }

        var embed = new EmbedBuilder()
        {
            Title = "10Bot Commands",
            Color = Color.Gold,
        };

        foreach (var item in Client.GetGuild(server.GuildID).GetApplicationCommandsAsync().Result)
            _ = embed.AddField(item.Name, item.Description);

        await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
    }
}
