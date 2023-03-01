using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class DiscordServerSettingsStorage : IService
{
    private readonly SettingsService Settings;

    public Dictionary<ulong, DiscordServerSettings> ServerSettings { get; }


    public DiscordServerSettingsStorage(SettingsService settings)
    {
        Settings = settings;
        ServerSettings = JsonConvert.DeserializeObject<Dictionary<ulong, DiscordServerSettings>>(File.ReadAllText(Settings.RootDirectory + (Settings.IsBeta ? "beta_config.json" : "config.json")))!;
    }
}
