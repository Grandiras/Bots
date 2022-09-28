namespace BotComponents;

public sealed class CustomCommand
{
    public string Name { get; set; }
    public string Description { get; set; }

    public CustomCommand(string name, string description)
    {
        Name = name;
        Description = description;
    }
}