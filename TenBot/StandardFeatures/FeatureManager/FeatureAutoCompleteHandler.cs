using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.StandardFeatures.FeatureManager;
public sealed class FeatureEnablementAutoCompleteHandler(FeatureService FeatureManager) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && !context.HasFeature(x)).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name))));
}

public sealed class FeatureDisablementAutoCompleteHandler(FeatureService FeatureManager) : ServerAutoCompleteHandler
{
    public override Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => Task.FromResult(AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && context.HasFeature(x)).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name))));
}