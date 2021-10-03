using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace _10Bot
{
    public class HandleSlashCommands
    {
        List<VoiceSettings> VoiceChannels;

        public HandleSlashCommands(List<VoiceSettings> voiceChannels)
        {
            VoiceChannels = voiceChannels;
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
        #region CommandHandler
        public async Task invite_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkOwnerOrModPattern(command).Result;
            if (!result.Item1) return;
            var settings = result.Item2;
            var user = result.Item3;

            var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
            await target.AddRoleAsync(settings.Role);
            await command.RespondAsync($"User {target.Username} erfolgreich zum Channel hinzugefügt.", ephemeral: true);
        }

        public async Task kick_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkOwnerOrModPattern(command).Result;
            if (!result.Item1) return;
            var settings = result.Item2;
            var user = result.Item3;

            var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
            await target.RemoveRoleAsync(settings.Role);
            await target.ModifyAsync(x => x.Channel = null);
            await command.RespondAsync($"User {target.Username} erfolgreich vom Channel gekickt.\nGrund: {((command.Data.Options.ElementAt(1).Value == null) ? "kein Grund" : (command.Data.Options.ElementAt(1).Value as string))} ", ephemeral: true);
        }

        public async Task manager_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkPattern(command).Result;
            if (!result.Item1) return;
            var settings = result.Item2;
            var user = result.Item3;

            if ((command.Data.Options.ElementAt(1).Value as IGuildUser) == user)
            {
                await command.RespondAsync("Ääääh, du bist hier Owner, ist dir schon klar, oder?", ephemeral: true);
                return;
            }

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "add":
                    var result3 = ValidateUserIsOwner(settings, command, user, true).Result;
                    if (!result3) return;
                    var target = subCommand.Options.First().Value as IGuildUser;
                    if (settings.Mods.Contains(target))
                    {
                        await command.RespondAsync($"User {target.Username} ist schon Moderator...", ephemeral: true);
                        break;
                    }
                    if (target.VoiceChannel == null || target.VoiceChannel.Id != settings.Channel.Id)
                    {
                        await command.RespondAsync($"User {target.Username} ist nicht in diesem Channel.", ephemeral: true);
                        break;
                    }
                    settings.Mods.Add(target);
                    await command.RespondAsync($"User {target.Username} erfolgreich zu einem Moderatoren dieses Channels gemacht.", ephemeral: true);
                    break;
                case "remove":
                    var result4 = ValidateUserIsOwner(settings, command, user, true).Result;
                    if (!result4) return;
                    var target2 = subCommand.Options.First().Value as IGuildUser;
                    if (settings.Mods.Contains(target2))
                    {
                        await command.RespondAsync($"User {target2.Username} ist kein Moderator...", ephemeral: true);
                        break;
                    }
                    settings.Mods.Add(target2);
                    await command.RespondAsync($"User {target2.Username} ist hier nun kein Moderator mehr.", ephemeral: true);
                    break;
                case "get":
                    if (settings.Mods.Count == 0)
                    {
                        await command.RespondAsync("Hier gibt es gar keine Mods...", ephemeral: true);
                        break;
                    }

                    StringBuilder modList = new StringBuilder();
                    foreach (var item in settings.Mods)
                    {
                        modList.Append("- ");
                        modList.Append(item.Username);
                        modList.Append("\n");
                    }
                    await command.RespondAsync(modList.ToString());
                    break;
                default:
                    break;
            }
            await command.RespondAsync("Irgendwas läuft hier schief... Bitte melden!", ephemeral: true);
        }

        public async Task owner_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkPattern(command).Result;
            if (!result.Item1) return;
            var settings = result.Item2;
            var user = result.Item3;

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "transfer":
                    var result3 = ValidateUserIsOwner(settings, command, user, true).Result;
                    if (!result3) return;
                    var target = subCommand.Options.First().Value as IGuildUser;
                    settings.Owner = target;
                    if (settings.Mods.Contains(target))
                    {
                        settings.Mods.Remove(target);
                    }
                    await command.RespondAsync($"Ownership erfolgreich an {target.Username} übertragen.");
                    break;
                case "get":
                    await command.RespondAsync($"Der aktuelle Owner hier ist {settings.Owner.Username}.", ephemeral: true);
                    break;
                default:
                    break;
            }
        }

        public async Task channel_CommandHandler(SocketSlashCommand command)
        {
            var result = TalkPattern(command).Result;
            if (!result.Item1) return;
            var settings = result.Item2;
            var user = result.Item3;

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "rename":
                    await settings.Channel.ModifyAsync(x => x.Name = (string)subCommand.Options.First().Value);
                    await settings.TextChannel.ModifyAsync(x => x.Name = ((string)subCommand.Options.First().Value).ToLower().Replace(" ", "-"));
                    await command.RespondAsync($"Der Name deines aktuellen Channels wurde erfolgreich auf {subCommand.Options.First().Value} gesetzt.");
                    break;
                default:
                    break;
            }
        }
        #endregion
        // -----
        #region Pattern Methods
        async Task<(bool, VoiceSettings, IGuildUser)> TalkPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            if (success) return (true, settings, user);
            else return (false, null, null);
        }

        async Task<(bool, PrivateVoiceSettings, IGuildUser)> PrivateTalkPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings voiceSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                voiceSettings = result2.Item2;
            }

            if (success) return (true, voiceSettings, user);
            else return (false, null, null);
        }

        async Task<(bool, PrivateVoiceSettings, IGuildUser)> PrivateTalkOwnerOrModPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings voiceSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                voiceSettings = result2.Item2;
            }
            
            if (success)
            {
                var userIsOwnerOrMod = ValidateUserIsOwnerOrMod(voiceSettings, command, user).Result;
                if (!userIsOwnerOrMod) success = false;
            }

            if (success) return (true, voiceSettings, user);
            else return (false, null, null);
        }

        async Task<(bool, PrivateVoiceSettings, IGuildUser)> PrivateTalkOwnerPattern(SocketSlashCommand command)
        {
            var success = true;

            var result = GetUserAndVoice(command).Result;
            var user = result.Item1;
            var settings = result.Item2;

            bool channelExists = ValidateChannelExists(settings, command, user).Result;
            if (!channelExists) success = false;

            PrivateVoiceSettings voiceSettings = null;

            if (success)
            {
                var result2 = ValidateChannelIsPrivate(settings, command).Result;
                if (!result2.Item1) success = false;
                voiceSettings = result2.Item2;
            }

            if (success)
            {
                var userIsOwnerOrMod = ValidateUserIsOwner(voiceSettings, command, user, true).Result;
                if (!userIsOwnerOrMod) success = false;
            }

            if (success) return (true, voiceSettings, user);
            else return (false, null, null);
        }
        #endregion
        // -----
        #region Validation Methods
        async Task<(IGuildUser, VoiceSettings)> GetUserAndVoice(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            VoiceSettings settings = VoiceChannels.Where(x => x.Channel.Id == user.VoiceChannel.Id).FirstOrDefault();

            return (user, settings);
        }

        async Task<bool> ValidateChannelExists(VoiceSettings settings, SocketSlashCommand command, IGuildUser user)
        {
            if (settings == null)
            {
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Voicechannel of user {user.Username} not found!");
                await command.RespondAsync("Sorry, dein aktueller Voicechannel wurde nicht gefunden... Bist du überhaupt in einem?", ephemeral: true);
                return false;
            }
            return true;
        }

        async Task<(bool, PrivateVoiceSettings)> ValidateChannelIsPrivate(VoiceSettings settings, SocketSlashCommand command)
        {
            PrivateVoiceSettings voiceSettings = settings as PrivateVoiceSettings;

            if (voiceSettings == null)
            {
                await command.RespondAsync("Sorry, aber das geht nur, wenn du in einem Private Talk bist...", ephemeral: true);
                return (false, voiceSettings);
            }
            return (true, voiceSettings);
        }

        async Task<bool> ValidateUserIsOwnerOrMod(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user)
        {
            if (ValidateUserIsOwner(settings, command, user, false).Result && settings.Mods.Where(x => x.Id == user.Id).FirstOrDefault() == null)
            {
                await command.RespondAsync("Du hast dafür keine Rechte! Du musst Owner dieses Channels sein oder zumindestens Mod, um diesen Command ausführen zu können.", ephemeral: true);
                return false;
            }
            return true;
        }

        async Task<bool> ValidateUserIsOwner(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user, bool answer)
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