using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class CustomCommands : IService
{
    private readonly Dictionary<ulong, List<CustomCommand>> Commands = new();

    private readonly SettingsService Settings;


    public CustomCommands(DiscordServerSettingsStorage serverSettings, SettingsService settings)
    {
        Settings = settings;
        
        foreach (var server in serverSettings.ServerSettings.Keys)
            Commands.Add(server, JsonConvert.DeserializeObject<List<CustomCommand>>(File.ReadAllText(Settings.RootDirectory + $"Servers/{server}/custom_commands.json"))!);
    }


    public bool CommandExists(string name, ulong guildID) => Commands[guildID].Any(c => c.Name == name);

    public CustomCommand? GetCommand(string name, ulong guildID) => Commands[guildID].FirstOrDefault(c => c.Name == name);
    public List<CustomCommand> GetCommands(ulong guildID) => Commands[guildID];

    public void AddCommand(CustomCommand command, ulong guildID)
    {
        Commands[guildID].Add(command);
        SaveCommands(guildID);
    }
    public void RemoveCommand(string name, ulong guildID)
    {
        _ = Commands[guildID].Remove(Commands[guildID].First(c => c.Name == name));
        SaveCommands(guildID);
    }

    public void ModifyCommand(string name, string newContent, ulong guildID)
    {
        Commands[guildID].First(c => c.Name == name).Content = newContent;
        SaveCommands(guildID);
    }

    private void SaveCommands(ulong guildID)
        => File.WriteAllText(Settings.RootDirectory + $"Servers/{guildID}/custom_commands.json",
                             JsonConvert.SerializeObject(Commands[guildID], Formatting.Indented));
}
