namespace TenBot.Models;
public sealed record ProjectTemplate(string Description, List<ProjectTemplateChannel> Channels);
public sealed record ProjectTemplateChannel(string Name, ProjectTemplateChannelKind Kind);

public enum ProjectTemplateChannelKind
{
    Text,
    Voice,
    Stage,
	Forum
}
