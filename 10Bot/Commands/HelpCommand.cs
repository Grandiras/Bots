using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class HelpCommand : InteractionModuleBase
{
    private readonly ServerService ServerService;


    public HelpCommand(ServerService serverService) => ServerService = serverService;


    [SlashCommand("help", "Displays a list of all commands and their descriptions existing here.")]
    public async Task RenameAsync()
    {
        var embed = new EmbedBuilder()
        {
            Title = "10Bot Commands",
            Color = Color.Gold,
        };

        foreach (var item in ServerService.GetServer(Context.Guild.Id)
                                          .GetApplicationCommandsAsync()
                                          .Result
                                          .Where(x => ((SocketGuildUser)Context.User).GuildPermissions.ToList().Any(y => x.DefaultMemberPermissions.Has(y))))
            _ = embed.AddField(item.Name, item.Description is not (null or "") ? item.Description : "[not provided]");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
