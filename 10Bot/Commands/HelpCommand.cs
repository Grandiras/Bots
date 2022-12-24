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

        var embed = new EmbedBuilder()
        {
            Title = "10Bot Commands",
            Color = Color.Gold,
        };

        foreach (var item in Client.GetGuild(server.GuildID)
                                   .GetApplicationCommandsAsync()
                                   .Result
                                   .Where(x => (Context.User as IGuildUser)!.GuildPermissions.ToList().Any(y => x.DefaultMemberPermissions.Has(y))))
            _ = embed.AddField(item.Name, item.Description is not (null or "") ? item.Description : "[not provided]");

        await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
    }
}
