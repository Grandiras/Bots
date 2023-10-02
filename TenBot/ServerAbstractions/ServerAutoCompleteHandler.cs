using Discord;
using Discord.Interactions;

namespace TenBot.ServerAbstractions;
public abstract class ServerAutoCompleteHandler : AutocompleteHandler
{
    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(IInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => await GenerateSuggestionsAsync((ServerInteractionContext)context, autocompleteInteraction, parameter, services);

    public abstract Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services);
}
