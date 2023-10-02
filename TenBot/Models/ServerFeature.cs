using Discord;
using Discord.Interactions;
using OneOf;
using TenBot.Features;
using TenBot.StandardFeatures;

namespace TenBot.Models;
public sealed class ServerFeature
{
    public required string Name { get; init; }
    public required string Description { get; init; }
    public required Color Color { get; init; }
    public required bool IsStandard { get; init; }
    public OneOf<IFeature, IStandardFeature> FeatureReference { get; init; }
    public bool RequiresSetup { get; init; } = false;
    public Func<ModuleInfo>? CommandHandlerModuleHandler { get; init; }
    public Type? SetupModalType { get; init; }
}
