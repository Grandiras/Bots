using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class CustomCommands
{
    private readonly List<CustomCommand> Commands = new();

    private readonly string FILE_PATH = Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/custom_commands.json";


    public CustomCommands()
    {
        var json = File.ReadAllText(FILE_PATH);
        Commands = JsonConvert.DeserializeObject<List<CustomCommand>>(json)!;
    }


    public bool CommandExists(string name) => Commands.Any(c => c.Name == name);

    public CustomCommand? GetCommand(string name) => Commands.FirstOrDefault(c => c.Name == name);
    public List<CustomCommand> GetCommands() => Commands;

    public void AddCommand(CustomCommand command)
    {
        Commands.Add(command);
        SaveCommands();
    }
    public void RemoveCommand(string name)
    {
        _ = Commands.Remove(Commands.First(c => c.Name == name));
        SaveCommands();
    }

    public void ModifyCommand(string name, string newContent)
    {
        Commands.First(c => c.Name == name).Content = newContent;
        SaveCommands();
    }

    private void SaveCommands()
    {
        var json = JsonConvert.SerializeObject(Commands);
        File.WriteAllText(FILE_PATH, json);
    }
}
