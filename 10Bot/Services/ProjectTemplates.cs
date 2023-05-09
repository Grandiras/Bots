using Discord.WebSocket;
using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ProjectTemplates : IService, IDisposable
{
	private const string DIRECTORY_NAME = "/ProjectTemplates";

	private readonly TenBotSettings Configuration;

	public Dictionary<string, ProjectTemplate> Templates { get; } = new();


	public ProjectTemplates(TenBotSettings configuration)
	{
		Configuration = configuration;

		foreach (var file in Directory.GetFiles(configuration.RootPath + DIRECTORY_NAME).Where(x => x.EndsWith(".json")))
			Templates.Add(file.Split(@"\").Select(x => x.Split("/")).Last().Last().Split(".json")[0], JsonConvert.DeserializeObject<ProjectTemplate>(File.ReadAllText(file))!);
	}


	public void CreateProjectTemplate(string name, string description, SocketCategoryChannel category)
	{
		var template = new ProjectTemplate(description, new());

		foreach (var channel in category.Channels.OrderBy(x => x.Position)) template.Channels.Add(CreateProjectTemplateChannel(channel!));

		Templates.Add(name, template);
		SaveTemplate(name);
	}
	private static ProjectTemplateChannel CreateProjectTemplateChannel(SocketGuildChannel channel) => channel switch
	{
		SocketStageChannel => new(channel.Name, ProjectTemplateChannelKind.Stage),
		SocketVoiceChannel => new(channel.Name, ProjectTemplateChannelKind.Voice),
		SocketTextChannel => new(channel.Name, ProjectTemplateChannelKind.Text),
		SocketForumChannel => new(channel.Name, ProjectTemplateChannelKind.Forum),
		_ => throw new NotSupportedException()
	};

	private void SaveTemplate(string name) => File.WriteAllText(Configuration.RootPath + $"{DIRECTORY_NAME}/{name}.json", JsonConvert.SerializeObject(Templates[name]));

	public void Dispose() => Templates.Keys.ToList().ForEach(SaveTemplate);
}
