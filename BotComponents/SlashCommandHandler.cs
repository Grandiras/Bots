using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;

namespace BotComponents
{
    public class SlashCommandHandler
    {
        protected List<VoiceSettings> VoiceChannels;
        protected Dictionary<string, string> LanguageTokens;

        public SlashCommandHandler(List<VoiceSettings> voiceChannels)
        {
            VoiceChannels = voiceChannels;
            LanguageTokens = StaticData.GetLanguageTokens("german");
        }

        public async Task FindCommandAsync(SocketSlashCommand command)
        {
            Type thisType = GetType();
            MethodInfo handler = thisType.GetMethod(command.CommandName + "_CommandHandler");
            Console.WriteLine($"User {command.User.Username} uses command '{command.CommandName}' (Method: {handler.Name})");
            if (handler == null)
            {
                await command.RespondAsync("Der Command funktioniert wohl nicht... Bitte melden!", ephemeral: true);
                return;
            }
            Task task = new Task(() => { handler.Invoke(this, new object[] { command }); });
            task.Start();
            task.Wait();
        }
        // -----
        #region Pattern Methods
        protected async Task<PatternResult<VoiceSettings>> TalkPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            if (success) return new PatternResult<VoiceSettings>(user, settings, success);
            else return new PatternResult<VoiceSettings>(success);
        }

        protected async Task<PatternResult<PrivateVoiceSettings>> PrivateTalkPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings privateSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                privateSettings = result2.Item2;
            }

            if (success) return new PatternResult<PrivateVoiceSettings>(user, privateSettings, success);
            else return new PatternResult<PrivateVoiceSettings>(success);
        }

        protected async Task<PatternResult<PrivateVoiceSettings>> PrivateTalkOwnerOrModPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings privateSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                privateSettings = result2.Item2;
            }

            if (success)
            {
                var userIsOwnerOrMod = ValidateUserIsOwnerOrMod(privateSettings, command, user).Result;
                if (!userIsOwnerOrMod) success = false;
            }

            if (success) return new PatternResult<PrivateVoiceSettings>(user, privateSettings, success);
            else return new PatternResult<PrivateVoiceSettings>(success);
        }

        protected async Task<PatternResult<PrivateVoiceSettings>> PrivateTalkOwnerPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings privateSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                privateSettings = result2.Item2;
            }

            if (success)
            {
                var userIsOwnerOrMod = ValidateUserIsOwner(privateSettings, command, user, true).Result;
                if (!userIsOwnerOrMod) success = false;
            }

            if (success) return new PatternResult<PrivateVoiceSettings>(user, privateSettings, success);
            else return new PatternResult<PrivateVoiceSettings>(success);
        }
        #endregion
        // -----
        #region Validation Methods
        protected async Task<(IGuildUser, VoiceSettings)> GetUserAndVoice(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            VoiceSettings settings = VoiceChannels.Where(x => x.Channel.Id == user.VoiceChannel.Id).FirstOrDefault();

            return (user, settings);
        }

        protected async Task<bool> ValidateChannelExists(VoiceSettings settings, SocketSlashCommand command, IGuildUser user)
        {
            if (settings == null)
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Voicechannel of user {user.Username} not found!");
                await command.RespondAsync("Sorry, dein aktueller Voicechannel wurde nicht gefunden... Bist du überhaupt in einem?", ephemeral: true);
                return false;
            }
            return true;
        }

        protected async Task<(bool, PrivateVoiceSettings)> ValidateChannelIsPrivate(VoiceSettings settings, SocketSlashCommand command)
        {
            PrivateVoiceSettings voiceSettings = settings as PrivateVoiceSettings;

            if (voiceSettings == null)
            {
                await command.RespondAsync("Sorry, aber das geht nur, wenn du in einem Private Talk bist...", ephemeral: true);
                return (false, voiceSettings);
            }
            return (true, voiceSettings);
        }

        protected async Task<bool> ValidateUserIsOwnerOrMod(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user)
        {
            if (ValidateUserIsOwner(settings, command, user, false).Result && settings.Mods.Where(x => x.Id == user.Id).FirstOrDefault() == null)
            {
                await command.RespondAsync("Du hast dafür keine Rechte! Du musst Owner dieses Channels sein oder zumindestens Mod, um diesen Command ausführen zu können.", ephemeral: true);
                return false;
            }
            return true;
        }

        protected async Task<bool> ValidateUserIsOwner(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user, bool answer)
        {
            if (user.Id != settings.Owner.Id)
            {
                if (answer) await command.RespondAsync("Du hast dafür keine Rechte! Du musst Owner dieses Channels sein, um diesen Command ausführen zu können.", ephemeral: true);
                return false;
            }
            return true;
        }
        #endregion
    }
}
