using Discord;
using Discord.Interactions;
using TenBot.Helpers;
using TenBot.Models;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.StandardFeatures.FeatureManager;
[Group("features", "View and manage all features available on this server."), DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class FeatureManagerCommand : InteractionModuleBase<ServerInteractionContext>, IStandardFeature
{
    private readonly FeatureService FeatureManager;

    public ServerFeature Feature => new()
    {
        Name = "Features",
        Description = "View and manage all features available on this server.",
        Color = Color.Blue,
        IsStandard = true,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureManager.GetModuleInfo<FeatureManagerCommand>
    };


    public FeatureManagerCommand(FeatureService featureManager) => FeatureManager = featureManager;


    [SlashCommand("list", "List all features available on this server.")]
    public async Task ListAsync()
    {
        // TODO embed provider with paging support
        var embed = new EmbedBuilder()
            .WithTitle("Available features")
            .WithColor(Feature.Color)
            .WithDescription("These are all features that are currently available on this server.");

        foreach (var feature in FeatureManager.GetFeatures())
            _ = embed.AddField(new EmbedFieldBuilder()
                .WithName(feature.Name + (feature.IsStandard ? " (Standard)" : "") + (feature.IsStandard || Context.HasFeature(feature) ? " - Enabled" : ""))
                .WithValue(feature.Description));

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("list-enabled", "List all features enabled on this server.")]
    public async Task ListEnabledAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Enabled features")
            .WithColor(Feature.Color)
            .WithDescription("These are all features that are currently enabled on this server.");

        foreach (var feature in Context.GetFeatures())
            _ = embed.AddField(new EmbedFieldBuilder()
                .WithName(feature.Name + (feature.IsStandard ? " (Standard)" : ""))
                .WithValue(feature.Description));

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("enable", "Enable a feature on this server.")]
    public async Task EnableAsync([Summary("feature", "The feature to enable."), Autocomplete(typeof(FeatureEnablementAutoCompleteHandler))] string featureName)
    {
        var feature = FeatureManager.GetFeatureByName(featureName).AsT0;

        if (feature.RequiresSetup && !Context.FeatureDataExists(feature))
            _ = this.InvokeGenericMethod(nameof(RespondWithModalAsync), feature.SetupModalType!, false, feature.SetupModalType!.Name, null);
        else
        {
            _ = Context.AddFeatureAsync(feature);
            _ = feature.FeatureReference.AsT0.AddForServerAsync(Context.ServerID);
            await RespondAsync($"Enabled feature '{feature.Name}' on this server.", ephemeral: true);
        }
    }

    [SlashCommand("disable", "Disable a feature on this server.")]
    public async Task DisableAsync([Summary("feature", "The feature to disable."), Autocomplete(typeof(FeatureDisablementAutoCompleteHandler))] string featureName,
                                   [Summary("reset", "Determine, whether all the data stored should be wiped.")] bool reset = false)
    {
        var feature = FeatureManager.GetFeatureByName(featureName).AsT0;

        await feature.FeatureReference.AsT0.RemoveForServerAsync(Context.ServerID, reset);
        _ = Context.RemoveFeatureAsync(feature);

        await RespondAsync($"Disabled feature '{feature.Name}' on this server. This action might take a few moments.", ephemeral: true);
    }

    [SlashCommand("disable-all", "Disable all features on this server.")]
    public async Task DisableAllAsync([Summary("reset", "Determine, whether all the data stored should be wiped.")] bool reset = true)
    {
        foreach (var feature in Context.GetFeatures())
        {
            await feature.FeatureReference.AsT0.RemoveForServerAsync(Context.ServerID, reset);
            _ = Context.RemoveFeatureAsync(feature);
        }

        await RespondAsync($"Disabled all features on this server. This action might take a few moments.", ephemeral: true);
    }
}
