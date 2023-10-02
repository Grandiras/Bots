using Discord;
using Discord.WebSocket;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesService : IFeature, IMustPostInitialize
{
    private readonly ServerService ServerManager;
    private readonly DiscordSocketClient Client;
    private readonly FeatureService FeatureManager;

    private readonly Random Randomizer = new();
    private readonly Dictionary<ulong, WelcomeMessageData> Messages = new();

    public ServerFeature Feature => new()
    {
        Name = "Welcome Messages",
        Description = "Sends a welcome message to new users.",
        Color = Color.Orange,
        IsStandard = false,
        FeatureReference = this,
        RequiresSetup = true,
        SetupModalType = typeof(WelcomeMessagesSetupModal),
        CommandHandlerModuleHandler = FeatureManager.GetModuleInfo<WelcomeMessagesCommand>
    };


    public WelcomeMessagesService(ServerService serverManager, DiscordSocketClient client, FeatureService featureManager)
    {
        ServerManager = serverManager;
        Client = client;
        FeatureManager = featureManager;

        Messages = ServerManager.ReadConcurrentFeatureDataWithKeysAsync<WelcomeMessageData>(Feature).Result.AsT0;
    }


    public Task PostInitializeAsync()
    {
        Client.UserJoined += SendWelcomeMessageAsync;
        return Task.CompletedTask;
    }

    public async Task SendWelcomeMessageAsync(SocketGuildUser user)
    {
        if (!ServerManager.HasFeature(user.Guild.Id, Feature)) return;

        var messages = Messages[user.Guild.Id].Messages;

        if (Messages[user.Guild.Id].ChannelId is 0 || messages.Count is 0) return;

        _ = await Client.GetGuild(user.Guild.Id).GetTextChannel(Messages[user.Guild.Id].ChannelId).SendMessageAsync(messages[Randomizer.Next(messages.Count)].Replace("[]", user.Mention));
    }

    public async Task AddServer(ulong id, ulong channelID)
    {
        Messages.Add(id, new() { ChannelId = channelID });
        await ServerManager.SaveFeatureDataAsync(id, Feature, Messages[id]);
    }
    public async Task AddForServerAsync(ulong id)
    {
        Messages.Add(id, (await ServerManager.ReadFeatureDataAsync<WelcomeMessageData>(id, Feature)).AsT0);
        await ServerManager.SaveFeatureDataAsync(id, Feature, Messages[id]);
    }
    public async Task RemoveForServerAsync(ulong id, bool reset)
    {
        _ = Messages.Remove(id);
        if (reset) await ServerManager.DeleteFeatureDataAsync(id, Feature);
    }

    public async Task AddMessage(ulong id, string message)
    {
        Messages[id].Messages.Add(message);
        await ServerManager.SaveFeatureDataAsync(id, Feature, Messages[id]);
    }
    public async Task RemoveMessage(ulong id, string message)
    {
        _ = Messages[id].Messages.Remove(message);
        await ServerManager.SaveFeatureDataAsync(id, Feature, Messages[id]);
    }
    public List<string> GetMessages(ulong id) => Messages[id].Messages;
}