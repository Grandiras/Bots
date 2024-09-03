using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesAutoCompleteHandler(WelcomeMessagesService WelcomeMessagesService) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(WelcomeMessagesService.GetMessages(context.ServerID).Take(25).Select(x => new AutocompleteResult(x, x))));
}
