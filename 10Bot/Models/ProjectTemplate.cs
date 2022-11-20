namespace TenBot.Models;
internal record ProjectTemplate(string Description, List<ProjectTemplateChannel> Channels);
internal record ProjectTemplateChannel(string Name, ProjectTemplateChannelKind Kind);

internal enum ProjectTemplateChannelKind
{
    Text,
    Voice
}
