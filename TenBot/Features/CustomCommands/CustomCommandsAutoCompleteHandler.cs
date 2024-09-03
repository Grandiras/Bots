using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.CustomCommands;
public sealed class CustomCommandsAutoCompleteHandler(CustomCommandsService CustomCommandsService) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(CustomCommandsService.GetCommands(context.ServerID).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name))));
}