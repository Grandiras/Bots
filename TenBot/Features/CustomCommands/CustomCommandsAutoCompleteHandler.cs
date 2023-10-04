using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.CustomCommands;
public sealed class CustomCommandsAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly CustomCommandsService CustomCommandsService;


    public CustomCommandsAutoCompleteHandler(CustomCommandsService customCommandsService) => CustomCommandsService = customCommandsService;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(CustomCommandsService.GetCommands(context.ServerID).Select(x => new AutocompleteResult(x.Name, x.Name)).Take(25));
}