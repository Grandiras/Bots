namespace TenBot.Features.CustomCommands;
public sealed class CustomCommandsData(string name, string content)
{
    public string Name { get; set; } = name;
    public string Content { get; set; } = content;
}
