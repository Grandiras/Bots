using Discord;
using System;
using System.Collections.Generic;
using System.Text;

namespace BotComponents
{
    public class PatternResult<T> where T : VoiceSettings
    {
        public IGuildUser User { get; private set; }
        public T Settings { get; private set; }
        public bool Succeeded { get; private set; }

        public PatternResult(bool succeeded) { Succeeded = succeeded; }
        public PatternResult(IGuildUser user, T settings, bool succeeded)
        {
            User = user;
            Settings = settings;
            Succeeded = succeeded;
        }
    }
}
