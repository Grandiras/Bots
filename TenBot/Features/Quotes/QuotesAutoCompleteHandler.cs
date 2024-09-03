using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.Quotes;
public sealed class QuotesAutoCompleteHandler(QuotesService QuotesService) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(QuotesService.GetQuotes(context.ServerID).Take(25).Select(x => new AutocompleteResult(x.Quote, x.Quote))));
}