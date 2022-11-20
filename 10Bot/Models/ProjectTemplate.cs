namespace TenBot.Models;
public record ProjectTemplate(string Description, List<ProjectTemplateChannel> Channels);
public record ProjectTemplateChannel(string Name, ProjectTemplateChannelKind Kind);

public enum ProjectTemplateChannelKind
{
    Text,
    Voice
}
