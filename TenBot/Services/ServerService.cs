using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using TenBot.Configuration;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ServerService : IService, IMustPostInitialize
{
    private const string SERVER_DATA_PATH = "Servers";

    private readonly DataService DataService;
    private readonly DiscordSocketClient Client;
    private readonly BotConfiguration Configuration;
    private readonly ILogger<ServerService> Logger;
    private readonly InteractionService Interactions;
    private readonly FeatureService FeatureManager;

    public List<Server> Servers { get; } = new();


    public ServerService(DataService dataService, DiscordSocketClient client, BotConfiguration configuration, ILogger<ServerService> logger, InteractionService interactions, FeatureService featureManager)
    {
        DataService = dataService;
        Client = client;
        Configuration = configuration;
        Logger = logger;
        Interactions = interactions;
        FeatureManager = featureManager;

        Servers = DataService.ReadFromConcurrentFilesAsync<Server>(SERVER_DATA_PATH, "config.json").Result.AsT0;
    }


    public async Task PostInitializeAsync()
    {
        Client.Guilds.ToList().ForEach(async x => { if (!Servers.Any(y => y.Id == x.Id)) await AddServerAsync(x); });

        Logger.LogInformation("Listening on {} servers", Servers.Count);

        await Task.CompletedTask;

        // TODO ensure this is called before the same method in ServerInteractionHandler
    }

    public async Task AddServerAsync(SocketGuild server, params ServerFeature[] features)
    {
        var serverModel = new Server
        {
            Name = server.Name,
            Id = server.Id,
            IsBeta = Configuration.IsBeta
        };
        serverModel.Features.AddRange(features.Select(x => x.Name));

        Servers.Add(serverModel);

        await DataService.SaveToFileAsync(Path.Combine(SERVER_DATA_PATH, server.Name, "config.json"), serverModel, true);
    }
    public async Task RemoveServerAsync(SocketGuild server)
    {
        _ = Servers.Remove(Servers.First(x => x.Id == server.Id));

        await DataService.DeleteDirectoryAsync(Path.Combine(SERVER_DATA_PATH, server.Name));
    }

    public OneOf<Server, NotFound> GetServerById(ulong id)
    {
        var server = Servers.FirstOrDefault(x => x.Id == id);
        return server is not null ? server : new NotFound();
    }
    public bool HasFeature(ulong id, ServerFeature feature)
    {
        var server = GetServerById(id);
        return !server.IsT1 && server.AsT0.Features.Any(x => x == feature.Name);
    }
    public bool FeatureDataForServerExists(ulong id, ServerFeature feature)
    {
        var server = GetServerById(id);
        return !server.IsT1 && DataService.FileExists(Path.Combine(SERVER_DATA_PATH, server.AsT0.Name, feature.Name + ".json"));
    }

    public OneOf<ulong, NotFound> HasChannel(ulong serverID, string name, ChannelType type)
        => Client.GetGuild(serverID).Channels.FirstOrDefault(x => x.Name == name && x.GetChannelType() == type) is IChannel channel
            ? channel.Id
            : new NotFound();

    public async Task<OneOf<Success, NotFound>> AddFeatureToServerAsync(ulong serverID, ServerFeature feature)
    {
        if (feature.IsStandard) return new Success();

        var server = Servers.FirstOrDefault(x => x.Id == serverID);

        if (server is null) return new NotFound();
        if (server.Features.Any(x => x == feature.Name)) return new NotFound();

        server.Features.Add(feature.Name);

        await DataService.SaveToFileAsync(Path.Combine(SERVER_DATA_PATH, server.Name, "config.json"), server, true);

        var featureModule = FeatureManager.GetFeatureModuleInfo(feature);
        if (featureModule.IsT0) _ = await Interactions.AddModulesToGuildAsync(serverID, false, featureModule.AsT0);

        return new Success();
    }
    public async Task<OneOf<Success, NotFound>> RemoveFeatureFromServerAsync(ulong serverID, ServerFeature feature)
    {
        if (feature.IsStandard) return new Success();

        var server = Servers.FirstOrDefault(x => x.Id == serverID);

        if (server is null) return new NotFound();
        if (!server.Features.Any(x => x == feature.Name)) return new NotFound();

        _ = server.Features.Remove(server.Features.First(x => x == feature.Name));

        await DataService.SaveToFileAsync(Path.Combine(SERVER_DATA_PATH, server.Name, "config.json"), server, true);

        var featureModule = FeatureManager.GetFeatureModuleInfo(feature);
        if (featureModule.IsT0) _ = await Interactions.RemoveModulesFromGuildAsync(serverID, featureModule.AsT0);

        return new Success();
    }

    public async Task<OneOf<T, NotFound>> ReadFeatureDataAsync<T>(ulong serverID, ServerFeature feature)
    {
        var server = Servers.FirstOrDefault(x => x.Id == serverID);

        if (server is null) return new NotFound();
        if (!server.Features.Any(x => x == feature.Name)) return new NotFound();

        var path = Path.Combine(SERVER_DATA_PATH, server.Name, feature.Name + ".json");

        return DataService.FileExists(path) ? await DataService.ReadFromFileAsync<T>(path) : new NotFound();
    }
    public async Task SaveFeatureDataAsync<T>(ulong serverID, ServerFeature feature, T data)
    {
        var server = Servers.FirstOrDefault(x => x.Id == serverID);

        if (server is null) return;
        if (!server.Features.Any(x => x == feature.Name)) return;

        await DataService.SaveToFileAsync(Path.Combine(SERVER_DATA_PATH, server.Name, feature.Name + ".json"), data, true);
    }
    public async Task DeleteFeatureDataAsync(ulong serverID, ServerFeature feature)
    {
        var server = Servers.FirstOrDefault(x => x.Id == serverID);

        if (server is null) return;
        if (!server.Features.Any(x => x == feature.Name)) return;

        await DataService.DeleteFileAsync(Path.Combine(SERVER_DATA_PATH, server.Name, feature.Name + ".json"));
    }

    public async Task<OneOf<List<T>, NotFound>> ReadConcurrentFeatureDataAsync<T>(ServerFeature feature)
    {
        var results = new List<T>();

        foreach (var server in Servers)
        {
            var result = await ReadFeatureDataAsync<T>(server.Id, feature);
            if (result.IsT1) return result.AsT1;

            results.Add(result.AsT0);
        }

        return results;
    }
    public async Task<OneOf<Dictionary<ulong, T>, NotFound>> ReadConcurrentFeatureDataWithKeysAsync<T>(ServerFeature feature)
    {
        var results = new Dictionary<ulong, T>();

        foreach (var server in Servers)
        {
            var result = await ReadFeatureDataAsync<T>(server.Id, feature);
            if (result.IsT1) continue;

            results.Add(server.Id, result.AsT0);
        }

        return results;
    }
}