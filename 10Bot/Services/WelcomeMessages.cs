using Discord;
using Newtonsoft.Json;

namespace TenBot.Services;
public sealed class WelcomeMessages : IService
{
    private const string WELCOME_MESSAGE_FILE = "welcome_messages.json";

    private readonly Dictionary<ulong, List<string>> WelcomeMessageDictionary = new();

    private readonly SettingsService Settings;
    private readonly FileSystemManager FileSystemManager;


    public WelcomeMessages(DiscordServerSettingsStorage serverSettings, SettingsService settings, FileSystemManager fileSystemManager)
    {
        Settings = settings;
        FileSystemManager = fileSystemManager;

        foreach (var server in serverSettings.ServerSettings.Keys)
        {
            FileSystemManager.CreateServerDirectoryIfNotExisting(server);
            WelcomeMessageDictionary.Add(server, JsonConvert.DeserializeObject<List<string>>(File.ReadAllText(Settings.RootDirectory + $"Servers/{server}/" + WELCOME_MESSAGE_FILE))!);
        }
    }


    public string GetWelcomeMessage(IGuildUser user)
    {
        var messages = GetWelcomeMessages(user.Guild.Id).ToList();
        return messages[Random.Shared.Next(messages.Count - 1)].Replace("[]", user.Mention);
    }

    public IEnumerable<string> GetWelcomeMessages(ulong guildID) => WelcomeMessageDictionary[guildID];

    public void AddWelcomeMessage(string message, ulong guildID)
    {
        var messages = GetWelcomeMessages(guildID).ToList();
        messages.Add(message);
        File.WriteAllText(Settings.RootDirectory + $"Servers/{guildID}/welcome_messages.json", JsonConvert.SerializeObject(messages, Formatting.Indented));
    }
}
