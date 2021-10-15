using Discord;
using Discord.WebSocket;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BotComponents;

namespace _10Bot
{
    public class _10BotSlashCommandHandler : SlashCommandHandler
    {
        public _10BotSlashCommandHandler(List<VoiceSettings> voiceChannels) : base(voiceChannels) { }
        // -----
        #region CommandHandler
        public async Task invite_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkOwnerOrModPattern(command).Result;
            if (!result.Succeeded) return;

            var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
            await target.AddRoleAsync(result.Settings.Role);
            await command.RespondAsync(LanguageTokens["invite_success"].Replace("{}", target.Username), ephemeral: true);
        }

        public async Task kick_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkOwnerOrModPattern(command).Result;
            if (!result.Succeeded) return;

            var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
            await target.RemoveRoleAsync(result.Settings.Role);
            await target.ModifyAsync(x => x.Channel = null);
            await command.RespondAsync(LanguageTokens["kick_success"].Replace("{}", target.Username).Replace("[]", (command.Data.Options.ElementAt(1).Value == null) ? "[unkwnow]" : (command.Data.Options.ElementAt(1).Value as string)), ephemeral: true);
            var dm = await target.CreateDMChannelAsync();
            await dm.SendMessageAsync(LanguageTokens["kick_reason"].Replace("{}", (command.Data.Options.ElementAt(1).Value == null) ? "[unkwnow]" : (command.Data.Options.ElementAt(1).Value as string)));
        }

        public async Task manager_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkPattern(command).Result;
            if (!result.Succeeded) return; 

            if ((command.Data.Options.ElementAt(1).Value as IGuildUser) == result.User)
            {
                await command.RespondAsync(LanguageTokens["manager_isOwner"], ephemeral: true);
                return;
            }

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "add":
                    var result3 = ValidateUserIsOwner(result.Settings, command, result.User, true).Result;
                    if (!result3) return;
                    var target = subCommand.Options.First().Value as IGuildUser;
                    if (result.Settings.Mods.Contains(target))
                    {
                        await command.RespondAsync(LanguageTokens["manager_add_already"].Replace("{}", target.Username), ephemeral: true);
                        break;
                    }
                    if (target.VoiceChannel == null || target.VoiceChannel.Id != result.Settings.Channel.Id)
                    {
                        await command.RespondAsync(LanguageTokens["manager_add_notInChannel"].Replace("{}", target.Username), ephemeral: true);
                        break;
                    }
                    result.Settings.Mods.Add(target);
                    await command.RespondAsync(LanguageTokens["manager_add_success"].Replace("{}", target.Username), ephemeral: true);
                    break;
                case "remove":
                    var result4 = ValidateUserIsOwner(result.Settings, command, result.User, true).Result;
                    if (!result4) return;
                    var target2 = subCommand.Options.First().Value as IGuildUser;
                    if (result.Settings.Mods.Contains(target2))
                    {
                        await command.RespondAsync(LanguageTokens["manager_remove_isNoMod"].Replace("{}", target2.Username), ephemeral: true);
                        break;
                    }
                    result.Settings.Mods.Add(target2);
                    await command.RespondAsync(LanguageTokens["manager_remove_success"].Replace("{}", target2.Username), ephemeral: true);
                    break;
                case "get":
                    if (result.Settings.Mods.Count == 0)
                    {
                        await command.RespondAsync(LanguageTokens["manager_get_noMods"], ephemeral: true);
                        break;
                    }

                    StringBuilder modList = new StringBuilder();
                    modList.Append(LanguageTokens["manager_get_heading"]);

                    foreach (var item in result.Settings.Mods)
                    {
                        modList.Append("- ");
                        modList.Append(item.Username);
                        modList.Append("\n");
                    }
                    await command.RespondAsync(modList.ToString());
                    break;
                default:
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'manager' (by {command.User.Username}) went wrong!");
                    await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                    break;
            }
        }

        public async Task owner_CommandHandler(SocketSlashCommand command)
        {
            var result = PrivateTalkPattern(command).Result;
            if (!result.Succeeded) return;

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "transfer":
                    var result3 = ValidateUserIsOwner(result.Settings, command, result.User, true).Result;
                    if (!result3) return;
                    var target = subCommand.Options.First().Value as IGuildUser;
                    result.Settings.Owner = target;
                    if (result.Settings.Mods.Contains(target))
                    {
                        result.Settings.Mods.Remove(target);
                    }
                    await command.RespondAsync(LanguageTokens["owner_transfer_success"].Replace("{}", target.Username) ,ephemeral: true);
                    break;
                case "get":
                    await command.RespondAsync(LanguageTokens["owner_get_success"].Replace("{}", result.Settings.Owner.Username), ephemeral: true);
                    break;
                default:
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'owner' (by {command.User.Username}) went wrong!");
                    await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                    break;
            }
        }

        public async Task channel_CommandHandler(SocketSlashCommand command)
        {
            var result = TalkPattern(command).Result;
            if (!result.Succeeded) return;

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "rename":
                    await result.Settings.Channel.ModifyAsync(x => x.Name = (string)subCommand.Options.First().Value);
                    await result.Settings.TextChannel.ModifyAsync(x => x.Name = ((string)subCommand.Options.First().Value).ToLower().Replace(" ", "-"));
                    await command.RespondAsync(LanguageTokens["channel_rename_success"].Replace("{}", result.Settings.Channel.Name), ephemeral: true);
                    break;
                default:
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'channel' (by {command.User.Username}) went wrong!");
                    await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                    break;
            }
        }
        
        public async Task command_CommandHandler(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            if (user == null)
            {
                await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
                return;
            }

            if (!user.RoleIds.Contains(Program.Instance.ModeratorRoleID))
            {
                await command.RespondAsync(LanguageTokens["command_noMod"], ephemeral: true);
                return;
            }

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "create":
                    if (Program.Instance.CustomCommands.Where(x => x.Name == (string)subCommand.Options.First().Value).FirstOrDefault() != null)
                    {
                        await command.RespondAsync(LanguageTokens["command_create_already"], ephemeral: true);
                        break;
                    }
                    Program.Instance.CustomCommands.Add(new CustomCommand((string)subCommand.Options.First().Value, (string)subCommand.Options.ElementAt(1).Value));
                    await Program.Instance.CreateCustomCommands();
                    await command.RespondAsync(LanguageTokens["command_create_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
                    break;
                case "delete":
                    if (Program.Instance.CustomCommands.Where(x => x.Name == (string)subCommand.Options.First().Value).FirstOrDefault() == null)
                    {
                        await command.RespondAsync(LanguageTokens["command_notExisting"], ephemeral: true);
                        break;
                    }
                    Program.Instance.CustomCommands.Remove(Program.Instance.CustomCommands.Where(x => x.Name == (string)subCommand.Options.First().Value).First());
                    await Program.Instance.CreateCustomCommands();
                    await command.RespondAsync(LanguageTokens["command_delete_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
                    break;
                case "modify":
                    if (Program.Instance.CustomCommands.Where(x => x.Name == (string)subCommand.Options.First().Value).FirstOrDefault() == null)
                    {
                        await command.RespondAsync(LanguageTokens["command_modify_notExisting"], ephemeral: true);
                        break;
                    }
                    Program.Instance.CustomCommands.Where(x => x.Name == (string)subCommand.Options.First().Value).First().Description = (string)subCommand.Options.ElementAt(1).Value;
                    await Program.Instance.CreateCustomCommands();
                    await command.RespondAsync(LanguageTokens["command_modify_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
                    break;
                default:
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'channel' (by {command.User.Username}) went wrong!");
                    await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                    break;
            }
        }

        public async Task execute_CommandHandler(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            if (user == null)
            {
                await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
                return;
            }

            var subCommand = command.Data.Options.FirstOrDefault();

            foreach (var item in Program.Instance.CustomCommands)
            {
                if (subCommand.Name == item.Name)
                {
                    await command.RespondAsync(item.Description);
                    return;
                }
            }

            await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
        }

        public async Task accept_CommandHandler(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            if (user == null)
            {
                await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
                return;
            }

            if (user.RoleIds.Contains(Program.Instance.MemberRoleID))
            {
                await command.RespondAsync(LanguageTokens["accept_already"], ephemeral: true);
                return;
            }

            await user.AddRoleAsync(Program.Instance.MemberRoleID);
            await command.RespondAsync(LanguageTokens["accept_success"], ephemeral: true);
        }

        public async Task help_CommandHandler(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            if (user == null)
            {
                await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
                return;
            }

            StringBuilder builder = new StringBuilder();

            builder.Append(LanguageTokens["help_heading"]);

            foreach (var item in Program.Instance.Guild.GetApplicationCommandsAsync().Result)
            {
                builder.Append($"- {item.Name}: {item.Description} \n");
            }

            await command.RespondAsync(builder.ToString(), ephemeral: true);
            return;
        }

        public async Task settings_CommandHandler(SocketSlashCommand command)
        {
            IGuildUser user = command.User as IGuildUser;
            if (user == null)
            {
                await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
                return;
            }

            if (!user.RoleIds.Contains(Program.Instance.ModeratorRoleID))
            {
                await command.RespondAsync(LanguageTokens["command_noMod"], ephemeral: true);
                return;
            }

            var subCommand = command.Data.Options.First();

            switch (subCommand.Name)
            {
                case "language":
                    if (StaticData.Language == (string)subCommand.Options.First().Value)
                    {
                        await command.RespondAsync(LanguageTokens["settings_language_already"], ephemeral: true);
                        return;
                    }
                    LanguageTokens = StaticData.GetLanguageTokens((string)subCommand.Options.First().Value);
                    Program.Instance.CreateSystemCommands();
                    Program.Instance.Settings["language"] = (string)subCommand.Options.First().Value;
                    await Program.Instance.UpdateSettings();
                    await command.RespondAsync(LanguageTokens["settings_language_success"].Replace("{}", StaticData.Language), ephemeral: true);
                    break;
                default:
                    Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'channel' (by {command.User.Username}) went wrong!");
                    await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                    break;
            }
        }
        #endregion
    }
}