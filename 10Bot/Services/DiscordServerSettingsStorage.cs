using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class DiscordServerSettingsStorage : IService
{
    public Dictionary<ulong, DiscordServerSettings> Settings { get; }


    public DiscordServerSettingsStorage()
        => Settings = JsonConvert.DeserializeObject<Dictionary<ulong, DiscordServerSettings>>(File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/beta_config.json"))!;
}
