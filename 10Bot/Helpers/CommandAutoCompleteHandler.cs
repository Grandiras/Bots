using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class CommandAutoCompleteHandler : AutocompleteHandler
{
    private readonly CustomCommands CustomCommands;


    public CommandAutoCompleteHandler(CustomCommands customCommands) => CustomCommands = customCommands;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context,
                                                                              IAutocompleteInteraction autocompleteInteraction,
                                                                              IParameterInfo parameter,
                                                                              IServiceProvider services)
    {
        var results = CustomCommands.GetCommands().Select(c => new AutocompleteResult(c.Name, c.Name));

        await Task.CompletedTask;

        return AutocompletionResult.FromSuccess(results.Take(25));
    }
}
