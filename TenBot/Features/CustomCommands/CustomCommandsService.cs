using Discord;
using OneOf;
using OneOf.Types;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.CustomCommands;
public sealed class CustomCommandsService : IFeature
{
    private readonly FeatureService FeatureService;
    private readonly ServerService ServerService;

    private readonly Dictionary<ulong, List<CustomCommandsData>> Commands = new();

    public ServerFeature Feature => new()
    {
        Name = "Custom Commands",
        Description = "Create custom commands for your server!",
        Color = Color.DarkMagenta,
        IsStandard = false,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureService.GetModuleInfo<CustomCommandCommand>
    };


    public CustomCommandsService(FeatureService featureService, ServerService serverService)
    {
        FeatureService = featureService;
        ServerService = serverService;

        Commands = ServerService.ReadConcurrentFeatureDataWithKeysAsync<List<CustomCommandsData>>(Feature).Result.AsT0;
    }


    public async Task AddForServerAsync(ulong id)
    {
        Commands.Add(id, (await ServerService.ReadFeatureDataAsync<List<CustomCommandsData>>(id, Feature)).Match(some => some, none => new()));
        await ServerService.SaveFeatureDataAsync(id, Feature, Commands[id]);
    }
    public async Task RemoveForServerAsync(ulong serverID, bool reset)
    {
        _ = Commands.Remove(serverID);
        if (reset) await ServerService.DeleteFeatureDataAsync(serverID, Feature);
    }

    public bool CommandExists(string name, ulong serverID) => Commands[serverID].Any(c => c.Name == name);

    public OneOf<CustomCommandsData, NotFound> GetCommand(string name, ulong serverID)
        => Commands[serverID].FirstOrDefault(c => c.Name == name) is CustomCommandsData data and not null
            ? data
            : new NotFound();
    public List<CustomCommandsData> GetCommands(ulong serverID) => Commands[serverID];

    public void AddCommand(CustomCommandsData command, ulong serverID)
    {
        Commands[serverID].Add(command);
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Commands[serverID]);
    }
    public void RemoveCommand(string name, ulong serverID)
    {
        _ = Commands[serverID].Remove(Commands[serverID].First(c => c.Name == name));
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Commands[serverID]);
    }
    public void ModifyCommand(string name, string newContent, ulong serverID)
    {
        Commands[serverID].First(c => c.Name == name).Content = newContent;
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Commands[serverID]);
    }
}
