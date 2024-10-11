namespace TenBot.Features.Projects;
public sealed record ProjectTemplate(string Name, string Description, List<ProjectDataChannel> Channels);
public sealed record ProjectDataChannel(string Name, ProjectDataChannelKind Kind);

public enum ProjectDataChannelKind
{
    Text,
    Voice,
    Stage,
    Forum,
    News
}
