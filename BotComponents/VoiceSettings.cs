using Discord;
using Discord.Rest;

namespace BotComponents;

public class VoiceSettings
{
    public RestVoiceChannel Channel { get; }
    public int TimesRenamed { get; set; }

    public VoiceSettings(RestVoiceChannel channel)
    {
        Channel = channel;
        TimesRenamed = 0;
    }
}

public sealed class PrivateVoiceSettings : VoiceSettings
{
    public RestRole Role { get; }
    public IGuildUser Owner { get; set; }
    public List<IGuildUser> Mods { get; set; }

    public PrivateVoiceSettings(RestVoiceChannel channel, RestRole role, IGuildUser owner) : base(channel)
    {
        Role = role;
        Owner = owner;
        Mods = new List<IGuildUser>();
    }
}
