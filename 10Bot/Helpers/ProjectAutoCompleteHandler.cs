using Discord;
using Discord.Interactions;
using TenBot.Enums;

namespace TenBot.Helpers;
internal class ProjectAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
                                                                        IAutocompleteInteraction autocompleteInteraction,
                                                                        IParameterInfo parameter,
                                                                        IServiceProvider services)
    {
        var results = Enum.GetValues<ProjectType>().Select(c => new AutocompleteResult(c.ToString(), c));

        await Task.CompletedTask;

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
