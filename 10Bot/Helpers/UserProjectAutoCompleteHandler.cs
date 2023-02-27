using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class UserProjectAutoCompleteHandler : AutocompleteHandler
{
    private readonly ServerService ServerService;


    public UserProjectAutoCompleteHandler(ServerService serverService) => ServerService = serverService;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
                                                                              IAutocompleteInteraction autocompleteInteraction,
                                                                              IParameterInfo parameter,
                                                                              IServiceProvider services)
    {
        var results = ServerService.GetRoles(x => x.Name.EndsWith(" - Project") || x.Name.EndsWith(" - Project - Public"), context.Guild.Id)
                                   .Where(x => ((IGuildUser)context.User).RoleIds.Contains(x.Id))
                                   .Select(x => new AutocompleteResult(x.Name.Split(" -").First(), x.Name.Split(" -").First()));

        await Task.CompletedTask;

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
