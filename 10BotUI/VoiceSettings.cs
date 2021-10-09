using Discord;
using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace _10BotUI
{
    public class VoiceSettings
    {
        public RestVoiceChannel Channel { get; set; }
        public RestTextChannel TextChannel { get; set; }
        public RestRole Role { get; set; }
        public int TimesRenamed { get; set; }

        public VoiceSettings(RestVoiceChannel channel, RestTextChannel textChannel, RestRole role)
        {
            Channel = channel;
            TextChannel = textChannel;
            Role = role;
            TimesRenamed = 0;
        }
    }

    public class PrivateVoiceSettings : VoiceSettings
    {
        public IGuildUser Owner { get; set; }
        public List<IGuildUser> Mods { get; set; }

        public PrivateVoiceSettings(RestVoiceChannel channel, RestTextChannel textChannel, RestRole role, IGuildUser owner) : base(channel, textChannel, role)
        {
            Owner = owner;
            Mods = new List<IGuildUser>();
        }
    }
}
