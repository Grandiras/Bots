using Discord;
using Discord.WebSocket;
using OneOf;
using OneOf.Types;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.Projects;
public sealed class ProjectService : IFeature
{
    private const string PROJECT_TEMPLATE_DATA_PATH = "ProjectTemplates";

    private readonly FeatureService FeatureService;
    private readonly ServerService ServerService;
    private readonly DataService DataService;

    private readonly Dictionary<ulong, List<ProjectTemplate>> Templates = [];

    public ServerFeature Feature => new()
    {
        Name = "Projects",
        Description = "Create and manage custom and temporary categories!",
        Color = Color.LightOrange,
        IsStandard = false,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureService.GetModuleInfo<ProjectCommand>
    };


    public ProjectService(FeatureService featureService, ServerService serverService, DataService dataService)
    {
        FeatureService = featureService;
        ServerService = serverService;
        DataService = dataService;

        var path = Path.Combine(PROJECT_TEMPLATE_DATA_PATH, "templates.json");
        if (!DataService.FileExists(path)) _ = DataService.SaveToFileAsync(path, new List<ProjectTemplate>(), true);

        Templates = ServerService.ReadConcurrentFeatureDataWithKeysAsync<List<ProjectTemplate>>(Feature).Result.AsT0;
        Templates.Add(0, DataService.ReadFromFileAsync<List<ProjectTemplate>>(path).Result.AsT0);
    }


    public async Task AddForServerAsync(ulong id)
    {
        Templates.Add(id, (await ServerService.ReadFeatureDataAsync<List<ProjectTemplate>>(id, Feature)).Match(some => some, none => []));
        await ServerService.SaveFeatureDataAsync(id, Feature, Templates[id]);
    }
    public async Task RemoveForServerAsync(ulong serverID, bool reset)
    {
        _ = Templates.Remove(serverID);
        if (reset) await ServerService.DeleteFeatureDataAsync(serverID, Feature);
    }

    public bool TemplateExists(string name, ulong serverID) => Templates[serverID].Any(c => c.Name == name);

    public OneOf<ProjectTemplate, NotFound> GetTemplate(string name, ulong serverID)
        => Templates[serverID].Concat(Templates[0]).FirstOrDefault(c => c.Name == name) is ProjectTemplate data and not null
            ? data
            : new NotFound();
    public List<ProjectTemplate> GetTemplates(ulong serverID, bool includeDefaults = true) => includeDefaults ? [.. Templates[serverID], .. Templates[0]] : Templates[serverID];

    public void AddTemplate(ProjectTemplate template, SocketCategoryChannel category)
    {
        var id = category.Guild.Id;

        foreach (var channel in category.Channels.OrderBy(x => x.Position)) template.Channels.Add(AddTemplateChannel(channel!));

        Templates[id].Add(template);
        _ = ServerService.SaveFeatureDataAsync(id, Feature, Templates[id]);
    }
    private static ProjectDataChannel AddTemplateChannel(SocketGuildChannel channel) => channel switch
    {
        SocketStageChannel => new(channel.Name, ProjectDataChannelKind.Stage),
        SocketVoiceChannel => new(channel.Name, ProjectDataChannelKind.Voice),
        SocketTextChannel => new(channel.Name, ProjectDataChannelKind.Text),
        SocketForumChannel => new(channel.Name, ProjectDataChannelKind.Forum),
        _ => throw new NotSupportedException()
    };
    public void RemoveTemplate(string name, ulong serverID)
    {
        _ = Templates[serverID].Remove(Templates[serverID].First(c => c.Name == name));
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Templates[serverID]);
    }
}
