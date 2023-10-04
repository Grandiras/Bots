using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly WelcomeMessagesService WelcomeMessagesService;


    public WelcomeMessagesAutoCompleteHandler(WelcomeMessagesService welcomeMessagesService) => WelcomeMessagesService = welcomeMessagesService;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(WelcomeMessagesService.GetMessages(context.ServerID).Take(25).Select(x => new AutocompleteResult(x, x)));
}
