using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class ProjectAutoCompleteHandler : AutocompleteHandler
{
    private readonly ServerService ServerService;


    public ProjectAutoCompleteHandler(ServerService serverService) => ServerService = serverService;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
                                                                              IAutocompleteInteraction autocompleteInteraction,
                                                                              IParameterInfo parameter,
                                                                              IServiceProvider services)
    {
        var results = ServerService.GetCategoriesByRoles(ServerService.GetRoles(x => x.Name.EndsWith(" - Project") || x.Name.EndsWith(" - Project - Public"), context.Guild.Id), context.Guild.Id)
                                   .Select(x => new AutocompleteResult(x.Name, x.Name));

        await Task.CompletedTask;

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
