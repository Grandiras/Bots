using Discord.Rest;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Text;

namespace _10Bot
{
    internal class VoiceSettings
    {
        public RestVoiceChannel Channel { get; set; }
        public RestTextChannel TextChannel { get; set; }
        public RestRole Role { get; set; }
        public List<SocketUser> Users { get; set; }
        public int TimesRenamed { get; set; }

        public VoiceSettings(RestVoiceChannel channel, RestTextChannel textChannel, RestRole role)
        {
            Channel = channel;
            TextChannel = textChannel;
            Role = role;
            Users = new List<SocketUser>();
            TimesRenamed = 0;
        }
    }

    internal class PrivateVoiceSettings : VoiceSettings
    {
        public SocketUser Owner { get; set; }

        public PrivateVoiceSettings(RestVoiceChannel channel, RestTextChannel textChannel, RestRole role, SocketUser owner) : base(channel, textChannel, role)
        {
            Owner = owner;
        }
    }
}
