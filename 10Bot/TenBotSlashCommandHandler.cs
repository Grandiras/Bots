using BotComponents;
using Discord;
using Discord.WebSocket;
using System.Text;

namespace TenBot;
internal sealed class TenBotSlashCommandHandler : CommandHandler, IDisposable
{
    public TenBotSlashCommandHandler() : base(new()) { }

    #region CommandHandler
    public async Task InviteCommandHandler(SocketSlashCommand command)
    {
        var result = PrivateTalkOwnerOrModPattern(command);
        if (!result.Succeeded) return;

        var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
        await target!.AddRoleAsync(result.Settings!.Role);
        await command.RespondAsync(LanguageTokens["invite_success"].Replace("{}", target.Username), ephemeral: true);
    }

    public async Task KickCommandHandler(SocketSlashCommand command)
    {
        var result = PrivateTalkOwnerOrModPattern(command);
        if (!result.Succeeded) return;

        var target = command.Data.Options.ElementAt(0).Value as IGuildUser;
        await target!.RemoveRoleAsync(result.Settings!.Role);
        await target.ModifyAsync(x => x.Channel = null);
        await command.RespondAsync(LanguageTokens["kick_success"].Replace("{}", target.Username).Replace("[]", (command.Data.Options.ElementAt(1).Value == null) ? "[unkwnow]" : (command.Data.Options.ElementAt(1).Value as string)), ephemeral: true);
        var dm = await target.CreateDMChannelAsync();
        _ = await dm.SendMessageAsync(LanguageTokens["kick_reason"].Replace("{}", (command.Data.Options.ElementAt(1).Value == null) ? "[unkwnow]" : (command.Data.Options.ElementAt(1).Value as string)));
    }

    public async Task ManagerCommandHandler(SocketSlashCommand command)
    {
        var result = PrivateTalkPattern(command);
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
                var result3 = ValidateUserIsOwner(result.Settings!, command, result.User!, true).Result;
                if (!result3) return;
                var target = subCommand.Options.First().Value as IGuildUser;
                if (result.Settings!.Mods.Contains(target!))
                {
                    await command.RespondAsync(LanguageTokens["manager_add_already"].Replace("{}", target!.Username), ephemeral: true);
                    break;
                }
                if (target!.VoiceChannel == null || target.VoiceChannel.Id != result.Settings.Channel.Id)
                {
                    await command.RespondAsync(LanguageTokens["manager_add_notInChannel"].Replace("{}", target.Username), ephemeral: true);
                    break;
                }
                result.Settings.Mods.Add(target);
                await command.RespondAsync(LanguageTokens["manager_add_success"].Replace("{}", target.Username), ephemeral: true);
                break;
            case "remove":
                var result4 = ValidateUserIsOwner(result.Settings!, command, result.User!, true).Result;
                if (!result4) return;
                var target2 = subCommand.Options.First().Value as IGuildUser;
                if (result.Settings!.Mods.Contains(target2!))
                {
                    await command.RespondAsync(LanguageTokens["manager_remove_isNoMod"].Replace("{}", target2!.Username), ephemeral: true);
                    break;
                }
                result.Settings.Mods.Add(target2!);
                await command.RespondAsync(LanguageTokens["manager_remove_success"].Replace("{}", target2!.Username), ephemeral: true);
                break;
            case "get":
                if (result.Settings!.Mods.Count == 0)
                {
                    await command.RespondAsync(LanguageTokens["manager_get_noMods"], ephemeral: true);
                    break;
                }

                var modList = new StringBuilder();
                _ = modList.Append(LanguageTokens["manager_get_heading"]);

                foreach (var item in result.Settings.Mods)
                {
                    _ = modList.Append("- ");
                    _ = modList.Append(item.Username);
                    _ = modList.Append('\n');
                }
                await command.RespondAsync(modList.ToString());
                break;
            default:
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'manager' (by {command.User.Username}) went wrong!");
                await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                break;
        }
    }

    public async Task OwnerCommandHandler(SocketSlashCommand command)
    {
        var result = PrivateTalkPattern(command);
        if (!result.Succeeded) return;

        var subCommand = command.Data.Options.First();

        switch (subCommand.Name)
        {
            case "transfer":
                var result3 = ValidateUserIsOwner(result.Settings!, command, result.User!, true).Result;
                if (!result3) return;
                var target = subCommand.Options.First().Value as IGuildUser;
                result.Settings!.Owner = target!;
                if (result.Settings.Mods.Contains(target!))
                {
                    _ = result.Settings.Mods.Remove(target!);
                }
                await command.RespondAsync(LanguageTokens["owner_transfer_success"].Replace("{}", target!.Username), ephemeral: true);
                break;
            case "get":
                await command.RespondAsync(LanguageTokens["owner_get_success"].Replace("{}", result.Settings!.Owner.Username), ephemeral: true);
                break;
            default:
                Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'owner' (by {command.User.Username}) went wrong!");
                await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
                break;
        }
    }

    public async Task CommandCommandHandler(SocketSlashCommand command)
    {
        //if (command.User is not IGuildUser user)
        //{
        //    await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
        //    return;
        //}

        //if (!user.RoleIds.Contains(Bot.Server.Settings.ModeratorRoleID))
        //{
        //    await command.RespondAsync(LanguageTokens["command_noMod"], ephemeral: true);
        //    return;
        //}

        //var subCommand = command.Data.Options.First();

        //switch (subCommand.Name)
        //{
        //    case "create":
        //        if (Bot.Services.GetComponent<CustomCommands>()?.Commands.FirstOrDefault(x => x.Name == (string)subCommand.Options.First().Value) != null)
        //        {
        //            await command.RespondAsync(LanguageTokens["command_create_already"], ephemeral: true);
        //            break;
        //        }
        //        Bot.Services.GetComponent<CustomCommands>()?.Commands.Add(new CustomCommand((string)subCommand.Options.First().Value, (string)subCommand.Options.ElementAt(1).Value));
        //        Bot.Services.GetComponent<CustomCommands>()?.CreateCustomCommandsAsync().Wait();
        //        await command.RespondAsync(LanguageTokens["command_create_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
        //        break;
        //    case "delete":
        //        if (Bot.Services.GetComponent<CustomCommands>()?.Commands.FirstOrDefault(x => x.Name == (string)subCommand.Options.First().Value) == null)
        //        {
        //            await command.RespondAsync(LanguageTokens["command_notExisting"], ephemeral: true);
        //            break;
        //        }
        //        _ = Bot.Services.GetComponent<CustomCommands>()?.Commands.Remove(Bot.Services.GetComponent<CustomCommands>()?.Commands.First(x => x.Name == (string)subCommand.Options.First().Value)!);
        //        Bot.Services.GetComponent<CustomCommands>()?.CreateCustomCommandsAsync().Wait();
        //        await command.RespondAsync(LanguageTokens["command_delete_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
        //        break;
        //    //case "modify":
        //    //    if (Bot.Services.GetComponent<CustomCommands>()?.Commands.FirstOrDefault(x => x.Name == (string)subCommand.Options.First().Value) == null)
        //    //    {
        //    //        await command.RespondAsync(LanguageTokens["command_modify_notExisting"], ephemeral: true);
        //    //        break;
        //    //    }
        //    //    Bot.Services.GetComponent<CustomCommands>()?.Commands.First(x => x.Name == (string)subCommand.Options.First().Value).Description = (string)subCommand.Options.ElementAt(1).Value;
        //    //    await Program.Instance.CreateCustomCommands();
        //    //    await command.RespondAsync(LanguageTokens["command_modify_success"].Replace("{}", (string)subCommand.Options.First().Value), ephemeral: true);
        //    //    break;
        //    default:
        //        Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Command 'channel' (by {command.User.Username}) went wrong!");
        //        await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
        //        break;
        //}
    }

    public async Task ExecuteCommandHandler(SocketSlashCommand command)
    {
        //if (command.User is not IGuildUser)
        //{
        //    await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
        //    return;
        //}

        //var subCommand = command.Data.Options.FirstOrDefault();

        //foreach (var item in Bot.Services.GetComponent<CustomCommands>()!.Commands)
        //{
        //    if (subCommand!.Name == item.Name)
        //    {
        //        await command.RespondAsync(item.Description);
        //        return;
        //    }
        //}

        //await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
    }

    public async Task SettingsCommandHandler(SocketSlashCommand command)
    {
        //if (command.User is not IGuildUser user)
        //{
        //    await command.RespondAsync(LanguageTokens["userNotFound"], ephemeral: true);
        //    return;
        //}

        //if (!user.RoleIds.Contains(Bot.Server.Settings.ModeratorRoleID))
        //{
        //    await command.RespondAsync(LanguageTokens["command_noMod"], ephemeral: true);
        //    return;
        //}
    }
    #endregion

    public void Dispose() => throw new NotImplementedException();
}