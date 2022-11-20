using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Models;

namespace TenBot.Helpers;
public sealed class ProjectAutoCompleteHandler : AutocompleteHandler
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;


    public ProjectAutoCompleteHandler(DiscordSocketClient client, DiscordServerSettings serverSettings)
    {
        Client = client;
        ServerSettings = serverSettings;
    }


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
                                                                              IAutocompleteInteraction autocompleteInteraction,
                                                                              IParameterInfo parameter,
                                                                              IServiceProvider services)
    {
        var results = Client.GetGuild(ServerSettings.GuildID).CategoryChannels.Where(x => x.PermissionOverwrites.Any(x => Client.GetGuild(ServerSettings.GuildID).Roles.Any(y => y.Name.EndsWith(" - Project") && y.Id == x.TargetId))).Select(x => new AutocompleteResult(x.Name, x.Name));

        await Task.CompletedTask;

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
