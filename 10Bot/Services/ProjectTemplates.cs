using Discord;
using Discord.WebSocket;
using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ProjectTemplates
{
    private readonly string FILE_PATH = Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/ProjectTemplates";

    public Dictionary<string, ProjectTemplate> Templates { get; } = new();


    public ProjectTemplates()
    {
        var files = Directory.GetFiles(FILE_PATH).Where(x => x.EndsWith(".json"));
        foreach (var file in files) Templates.Add(file.Split(@"\").Last().Split(".json")[0],
                                                  JsonConvert.DeserializeObject<ProjectTemplate>(File.ReadAllText(file))!);
    }


    public void CreateProjectTemplate(string name, string description, SocketCategoryChannel category)
    {
        var channels = new List<ProjectTemplateChannel>();
        foreach (var channel in category.Channels.OrderBy(x => x.Position))
        {
            var channelTemplate = CreateProjectTemplateChannel(channel!);
            if (channelTemplate is not null) channels.Add(channelTemplate);
        }

        var template = new ProjectTemplate(description, channels);

        Templates.Add(name, template);
        File.WriteAllText($"{FILE_PATH}/{name}.json", JsonConvert.SerializeObject(template));
    }
    private static ProjectTemplateChannel? CreateProjectTemplateChannel(SocketGuildChannel channel) => channel switch
    {
        SocketStageChannel => new(channel.Name, ProjectTemplateChannelKind.Stage),
        SocketVoiceChannel => new(channel.Name, ProjectTemplateChannelKind.Voice),
        SocketTextChannel => new(channel.Name, ProjectTemplateChannelKind.Text),
        _ => null
    };
}
