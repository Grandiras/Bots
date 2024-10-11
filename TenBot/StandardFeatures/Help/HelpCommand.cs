using Discord;
using Discord.Interactions;
using TenBot.Models;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.StandardFeatures.Help;
public sealed class HelpCommand(FeatureService FeatureManager) : InteractionModuleBase<ServerInteractionContext>, IStandardFeature
{
    public ServerFeature Feature => new()
    {
        Name = "Help",
        Description = "Provides a list of all commands and their descriptions existing here.",
        Color = Color.Gold,
        IsStandard = true,
        CommandHandlerModuleHandler = FeatureManager.GetModuleInfo<HelpCommand>
    };

    [SlashCommand("help", "Provides a list of all commands and their descriptions existing here.")]
    public async Task ListAsync()
    {
        var embed = new EmbedBuilder()
        {
            Title = "10Bot Commands",
            Color = Feature.Color,
        };

        foreach (var item in await Context.GetApplicationCommandsForUserAsync(Context.User)) _ = embed.AddField(item.Name, item.Description is not (null or "") ? item.Description : "[not provided]");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
