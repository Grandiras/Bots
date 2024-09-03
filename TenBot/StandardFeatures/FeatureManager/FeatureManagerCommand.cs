using Discord;
using Discord.Interactions;
using TenBot.Helpers;
using TenBot.Models;
using TenBot.ServerAbstractions;
using TenBot.Services;

namespace TenBot.StandardFeatures.FeatureManager;
[Group("features", "View and manage all features available on this server."), DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class FeatureManagerCommand(FeatureService FeatureManager) : InteractionModuleBase<ServerInteractionContext>, IStandardFeature
{
    public ServerFeature Feature => new()
    {
        Name = "Features",
        Description = "View and manage all features available on this server.",
        Color = Color.Blue,
        IsStandard = true,
        CommandHandlerModuleHandler = FeatureManager.GetModuleInfo<FeatureManagerCommand>
    };


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
        var feature = FeatureManager.GetFeatureByName(featureName);

        if (feature.IsT1)
        {
            await RespondAsync($"Feature '{featureName}' not found!", ephemeral: true);
            return;
        }

        if (feature.AsT0.RequiresSetup && !Context.FeatureDataExists(feature.AsT0))
            _ = this.InvokeGenericMethod(nameof(RespondWithModalAsync), feature.AsT0.SetupModalType!, false, feature.AsT0.SetupModalType!.Name, null, null);

        else
        {
            _ = Context.AddFeatureAsync(feature.AsT0);
            _ = feature.AsT0.FeatureReference.AsT0.AddForServerAsync(Context.ServerID);
            await RespondAsync($"Enabled feature '{feature.AsT0.Name}' on this server.", ephemeral: true);
        }
    }

    [SlashCommand("disable", "Disable a feature on this server.")]
    public async Task DisableAsync([Summary("feature", "The feature to disable."), Autocomplete(typeof(FeatureDisablementAutoCompleteHandler))] string featureName,
                                   [Summary("reset", "Determine, whether all the data stored should be wiped.")] bool reset = false)
    {
        var feature = FeatureManager.GetFeatureByName(featureName);

        if (feature.IsT1)
        {
            await RespondAsync($"Feature '{featureName}' not found!", ephemeral: true);
            return;
        }

        await feature.AsT0.FeatureReference.AsT0.RemoveForServerAsync(Context.ServerID, reset);
        _ = Context.RemoveFeatureAsync(feature.AsT0);

        await RespondAsync($"Disabled feature '{feature.AsT0.Name}' on this server. This action might take a few moments.", ephemeral: true);
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

    // TODO: Add a command to view feature details, to reset feature data, to reinitialize feature
}
