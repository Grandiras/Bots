using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.StandardFeatures.FeatureManager;
public sealed class FeatureEnablementAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly FeatureService FeatureManager;


    public FeatureEnablementAutoCompleteHandler(FeatureService featureManager) => FeatureManager = featureManager;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && !context.HasFeature(x)).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name)));
}

public sealed class FeatureDisablementAutoCompleteHandler : ServerAutoCompleteHandler
{
    private readonly FeatureService FeatureManager;


    public FeatureDisablementAutoCompleteHandler(FeatureService featureManager) => FeatureManager = featureManager;


    public override async Task<AutocompletionResult> GenerateSuggestionsAsync(ServerInteractionContext context, IAutocompleteInteraction autocompleteInteraction, IParameterInfo parameter, IServiceProvider services)
        => AutocompletionResult.FromSuccess(FeatureManager.GetFeatures().Where(x => !x.IsStandard && context.HasFeature(x)).Take(25).Select(x => new AutocompleteResult(x.Name, x.Name)));
}