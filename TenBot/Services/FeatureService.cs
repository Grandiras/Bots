using Discord.Interactions;
using OneOf;
using OneOf.Types;
using TenBot.Features;
using TenBot.Helpers;
using TenBot.Models;
using TenBot.ServerAbstractions;
using TenBot.StandardFeatures;

namespace TenBot.Services;
public sealed class FeatureService(IServiceProvider Services, InteractionService Interactions) : IService
{
    public IEnumerable<ServerFeature> GetFeatures() => Services.GetAllServicesWith<IStandardFeature>().Select(x => x.Feature).Concat(Services.GetAllServicesWith<IFeature>().Select(x => x.Feature));
    public IEnumerable<ServerFeature> GetFeaturesForServer(Server server) => GetFeatures().Where(x => x.IsStandard || server.Features.Any(y => y == x.Name));
    public OneOf<ServerFeature, NotFound> GetFeatureByName(string name)
    {
        var feature = GetFeatures().FirstOrDefault(x => x.Name == name);
        return feature is not null ? feature : new NotFound();
    }

    public IEnumerable<ModuleInfo> GetFeatureModuleInfoForServer(Server server)
        => GetFeaturesForServer(server).Where(x => x.CommandHandlerModuleHandler is not null).Select(x => x.CommandHandlerModuleHandler!());
    public static OneOf<ModuleInfo, No> GetFeatureModuleInfo(ServerFeature feature) => feature.CommandHandlerModuleHandler is not null ? feature.CommandHandlerModuleHandler() : new No();

    public ModuleInfo GetModuleInfo<T>() where T : InteractionModuleBase<ServerInteractionContext> => Interactions.GetModuleInfo<T>();
}
