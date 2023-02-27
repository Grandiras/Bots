using Discord;
using Newtonsoft.Json;

namespace TenBot.Services;
public sealed class WelcomeMessages : IService
{
    private readonly Dictionary<ulong, List<string>> WelcomeMessageDictionary = new();


    public WelcomeMessages(DiscordServerSettingsStorage serverSettings)
    {
        foreach (var server in serverSettings.Settings.Keys)
            WelcomeMessageDictionary.Add(server,
                                         JsonConvert.DeserializeObject<List<string>>(
                                             File.ReadAllText(Directory.GetCurrentDirectory() + $"/Data/Servers/{server}/welcome_messages.json"))!);
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
        File.WriteAllText(Directory.GetCurrentDirectory() + $"/Data/Servers/{guildID}/welcome_messages.json",
                          JsonConvert.SerializeObject(messages, Formatting.Indented));
    }
}
