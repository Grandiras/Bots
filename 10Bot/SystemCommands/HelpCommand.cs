using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace TenBot.SystemCommands;
public class HelpCommand : InteractionModuleBase
{
    private readonly DiscordServerSettings ServerSettings;
    private readonly DiscordSocketClient Client;


    public HelpCommand(DiscordServerSettings serverSettings, DiscordSocketClient client)
    {
        ServerSettings = serverSettings;
        Client = client;
    }


    [SlashCommand("help", "Displayes a list of all commands and their descriptions existing here.")]
    public async Task RenameAsync()
    {
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

        foreach (var item in Client.GetGuild(ServerSettings.GuildID).GetApplicationCommandsAsync().Result)
            _ = embed.AddField(item.Name, item.Description);

        await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
    }
}
