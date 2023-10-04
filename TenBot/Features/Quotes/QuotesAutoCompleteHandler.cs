using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.Quotes;
public sealed class QuotesAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly QuotesService QuotesService;


    public QuotesAutoCompleteHandler(QuotesService quotesService) => QuotesService = quotesService;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(QuotesService.GetQuotes(context.ServerID).Take(25).Select(x => new AutocompleteResult(x.Quote, x.Quote)));
}