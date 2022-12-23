using TenBot.Models;

namespace TenBot.Services;
public sealed class DiscordServerSettingsStorage
{
    public Dictionary<ulong, DiscordServerSettings> Settings { get; }


    public DiscordServerSettingsStorage(Dictionary<ulong, DiscordServerSettings> settings) => Settings = settings;
}
