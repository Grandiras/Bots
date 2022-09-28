using Discord;
using Discord.WebSocket;

namespace BotComponents;
public class CommandHandler
{
    protected List<VoiceSettings> VoiceChannels;
    public Dictionary<string, string> LanguageTokens { get; protected set; }

    public CommandHandler(List<VoiceSettings> voiceChannels)
    {
        VoiceChannels = voiceChannels;
        LanguageTokens = StaticData.GetLanguageTokens("english");
    }

    public void ChangeLanguage(string language) => LanguageTokens = StaticData.GetLanguageTokens(language);

    public async Task FindCommandAsync(SocketSlashCommand command)
    {
        var thisType = GetType();
        var handler = thisType.GetMethod(command.CommandName[0].ToString().ToUpper() + command.CommandName[1..] + "CommandHandler");
        Console.WriteLine($"User {command.User.Username} uses command '{command.CommandName}' (Method: {handler!.Name})");
        if (handler == null)
        {
            await command.RespondAsync(LanguageTokens["failed"], ephemeral: true);
            return;
        }
        var task = new Task(() => handler.Invoke(this, new object[] { command }));
        task.Start();
        task.Wait();
    }
    // -----
    #region Pattern Methods
    protected PatternResult<VoiceSettings> TalkPattern(SocketSlashCommand command)
    {
        var success = true;

        var result = GetUserAndVoice(command);
        var user = result.Item1;
        var settings = result.Item2;

        var channelExists = ValidateChannelExists(settings, command, user).Result;
        if (!channelExists) success = false;

        return success ? new PatternResult<VoiceSettings>(user, settings, success) : new PatternResult<VoiceSettings>(success);
    }

    protected PatternResult<PrivateVoiceSettings> PrivateTalkPattern(SocketSlashCommand command)
    {
        var success = true;

        var result = GetUserAndVoice(command);
        var user = result.Item1;
        var settings = result.Item2;

        var channelExists = ValidateChannelExists(settings, command, user).Result;
        if (!channelExists) success = false;

        PrivateVoiceSettings? privateSettings = null;

        if (success)
        {
            var result2 = ValidateChannelIsPrivate(settings, command).Result;
            if (!result2.Item1) success = false;
            privateSettings = result2.Item2;
        }

        return success
            ? new PatternResult<PrivateVoiceSettings>(user, privateSettings!, success)
            : new PatternResult<PrivateVoiceSettings>(success);
    }

    protected PatternResult<PrivateVoiceSettings> PrivateTalkOwnerOrModPattern(SocketSlashCommand command)
    {
        var success = true;

        var result = GetUserAndVoice(command);
        var user = result.Item1;
        var settings = result.Item2;

        var channelExists = ValidateChannelExists(settings, command, user).Result;
        if (!channelExists) success = false;

        PrivateVoiceSettings? privateSettings = null;

        if (success)
        {
            var result2 = ValidateChannelIsPrivate(settings, command).Result;
            if (!result2.Item1) success = false;
            privateSettings = result2.Item2;
        }

        if (success)
        {
            var userIsOwnerOrMod = ValidateUserIsOwnerOrMod(privateSettings!, command, user).Result;
            if (!userIsOwnerOrMod) success = false;
        }

        return success
            ? new PatternResult<PrivateVoiceSettings>(user, privateSettings!, success)
            : new PatternResult<PrivateVoiceSettings>(success);
    }

    protected PatternResult<PrivateVoiceSettings> PrivateTalkOwnerPattern(SocketSlashCommand command)
    {
        var success = true;

        var result = GetUserAndVoice(command);
        var user = result.Item1;
        var settings = result.Item2;

        var channelExists = ValidateChannelExists(settings, command, user).Result;
        if (!channelExists) success = false;

        PrivateVoiceSettings? privateSettings = null;

        if (success)
        {
            var result2 = ValidateChannelIsPrivate(settings, command).Result;
            if (!result2.Item1) success = false;
            privateSettings = result2.Item2;
        }

        if (success)
        {
            var userIsOwnerOrMod = ValidateUserIsOwner(privateSettings!, command, user, true).Result;
            if (!userIsOwnerOrMod) success = false;
        }

        return success
            ? new PatternResult<PrivateVoiceSettings>(user, privateSettings!, success)
            : new PatternResult<PrivateVoiceSettings>(success);
    }
    #endregion
    // -----
    #region Validation Methods
    protected (IGuildUser, VoiceSettings) GetUserAndVoice(SocketSlashCommand command)
    {
        var user = command.User as IGuildUser;
        var settings = VoiceChannels.FirstOrDefault(x => x.Channel.Id == user!.VoiceChannel.Id);

        return (user, settings)!;
    }

    protected async Task<bool> ValidateChannelExists(VoiceSettings settings, SocketSlashCommand command, IGuildUser user)
    {
        if (settings == null)
        {
            Console.WriteLine($"[{DateTime.Now.TimeOfDay}] Voicechannel of user {user.Username} not found!");
            await command.RespondAsync(LanguageTokens["userVoiceNotFound"], ephemeral: true);
            return false;
        }
        return true;
    }

    protected async Task<(bool, PrivateVoiceSettings)> ValidateChannelIsPrivate(VoiceSettings settings, SocketSlashCommand command)
    {
        var voiceSettings = settings as PrivateVoiceSettings;

        if (voiceSettings == null)
        {
            await command.RespondAsync(LanguageTokens["userNotInPrivateTalk"], ephemeral: true);
            return (false, voiceSettings)!;
        }
        return (true, voiceSettings);
    }

    protected async Task<bool> ValidateUserIsOwnerOrMod(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user)
    {
        if (ValidateUserIsOwner(settings, command, user, false).Result && settings.Mods.FirstOrDefault(x => x.Id == user.Id) == null)
        {
            await command.RespondAsync(LanguageTokens["userNotModOrOwner"], ephemeral: true);
            return false;
        }
        return true;
    }

    protected async Task<bool> ValidateUserIsOwner(PrivateVoiceSettings settings, SocketSlashCommand command, IGuildUser user, bool answer)
    {
        if (user.Id != settings.Owner.Id)
        {
            if (answer) await command.RespondAsync(LanguageTokens["userNotOwner"], ephemeral: true);
            return false;
        }
        return true;
    }
    #endregion
}
