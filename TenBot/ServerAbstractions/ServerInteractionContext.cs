using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using OneOf;
using OneOf.Types;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.ServerAbstractions;
public sealed class ServerInteractionContext(DiscordSocketClient Client, SocketInteraction Interaction, ServerService ServerManager, FeatureService FeatureManager) : SocketInteractionContext(Client, Interaction)
{
    public ulong ServerID { get; } = Interaction.GuildId!.Value;

    public new SocketGuildUser User => (SocketGuildUser)Interaction.User;


    public Server GetServer() => ServerManager.GetServerById(ServerID).AsT0;
    public bool HasFeature(ServerFeature feature) => ServerManager.HasFeature(ServerID, feature);
    public bool FeatureDataExists(ServerFeature feature) => ServerManager.FeatureDataForServerExists(ServerID, feature);

    public OneOf<ulong, NotFound> HasChannel(string name, ChannelType type) => ServerManager.HasChannel(ServerID, name, type);

    public async Task<OneOf<Success, NotFound>> AddFeatureAsync(ServerFeature feature) => await ServerManager.AddFeatureToServerAsync(ServerID, feature);
    public async Task<OneOf<Success, NotFound>> RemoveFeatureAsync(ServerFeature feature) => await ServerManager.RemoveFeatureFromServerAsync(ServerID, feature);

    public async Task<OneOf<T, NotFound>> ReadFeatureDataAsync<T>(ServerFeature feature) => await ServerManager.ReadFeatureDataAsync<T>(ServerID, feature);
    public async Task SaveFeatureDataAsync<T>(ServerFeature feature, T data) => await ServerManager.SaveFeatureDataAsync(ServerID, feature, data);
    public async Task DeleteFeatureDataAsync(ServerFeature feature) => await ServerManager.DeleteFeatureDataAsync(ServerID, feature);

    public IEnumerable<ServerFeature> GetFeatures() => FeatureManager.GetFeaturesForServer(GetServer());

    public async Task<IEnumerable<SocketApplicationCommand>> GetApplicationCommandsAsync() => await Client.GetGuild(ServerID).GetApplicationCommandsAsync();
    public async Task<IEnumerable<SocketApplicationCommand>> GetApplicationCommandsForUserAsync(SocketGuildUser user)
        => (await Client.GetGuild(ServerID).GetApplicationCommandsAsync()).Where(x => user.GuildPermissions.ToList().Any(y => x.DefaultMemberPermissions.Has(y)));
}
