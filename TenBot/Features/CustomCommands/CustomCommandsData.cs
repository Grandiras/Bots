namespace TenBot.Features.CustomCommands;
public sealed class CustomCommandsData
{
    public string Name { get; set; }
    public string Content { get; set; }


    public CustomCommandsData(string name, string content)
    {
        Name = name;
        Content = content;
    }
}
