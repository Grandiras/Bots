using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.Helpers;
public sealed class FeatureEnablementAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly FeatureService FeatureManager;


    public FeatureEnablementAutoCompleteHandler(FeatureService featureManager) => FeatureManager = featureManager;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && !context.HasFeature(x)).Select(x => new AutocompleteResult(x.Name, x.Name)).Take(25));
}

public sealed class FeatureDisablementAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly FeatureService FeatureManager;


    public FeatureDisablementAutoCompleteHandler(FeatureService featureManager) => FeatureManager = featureManager;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && context.HasFeature(x)).Select(x => new AutocompleteResult(x.Name, x.Name)).Take(25));
}