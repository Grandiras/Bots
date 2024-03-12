using Discord;
using Discord.WebSocket;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.VoiceManager;
public class VoiceManagerService : IFeature, IMustPostInitialize
{
    private readonly ServerService ServerManager;
    private readonly DiscordSocketClient Client;
    private readonly FeatureService FeatureManager;

    private readonly Dictionary<ulong, VoiceManagerData> Data = [];

    public ServerFeature Feature => new()
    {
        Name = "Voice Manager",
        Description = "Manages voice channels dynamically.",
        Color = Color.Green,
        IsStandard = false,
        FeatureReference = this,
        RequiresSetup = true,
        SetupModalType = typeof(VoiceManagerSetupModal),
        CommandHandlerModuleHandler = FeatureManager.GetModuleInfo<VoiceManagerCommand>
    };


    public VoiceManagerService(ServerService serverManager, DiscordSocketClient client, FeatureService featureManager)
    {
        ServerManager = serverManager;
        Client = client;
        FeatureManager = featureManager;

        Data = ServerManager.ReadConcurrentFeatureDataWithKeysAsync<VoiceManagerData>(Feature).Result.AsT0;
    }


    public async Task PostInitializeAsync()
    {
        Client.UserVoiceStateUpdated += (user, oldVoice, newVoice) => HandleVoiceStateUpdateAsync((SocketGuildUser)user, oldVoice.VoiceChannel, newVoice.VoiceChannel);
        await Task.CompletedTask;
    }

    public async Task HandleVoiceStateUpdateAsync(SocketGuildUser user, SocketVoiceChannel? oldVoice, SocketVoiceChannel? newVoice)
    {
        if (!ServerManager.HasFeature(user.Guild.Id, Feature)) return;

        if (oldVoice is not null && oldVoice.Category.Id == Data[oldVoice.Guild.Id].VoiceChannelCategory && oldVoice.ConnectedUsers.Count is 0) await CleanUpChannelAsync(oldVoice);

        if (newVoice is not null && newVoice.Category.Id == Data[newVoice.Guild.Id].VoiceCreationCategory)
        {
            if (newVoice.Id == Data[newVoice.Guild.Id].NewVoiceChannel) await CreateNewVoiceAsync(user);
            else if (newVoice.Id == Data[newVoice.Guild.Id].NewPrivateVoiceChannel) await CreateNewPrivateVoiceAsync(user); // TODO laggy
        }
    }

    public async Task AddServerAsync(ulong id, VoiceManagerSetupModal modal)
    {
        Data.Add(id, new()
        {
            VoiceCreationCategory = 0,
            NewVoiceChannel = 0,
            NewPrivateVoiceChannel = 0,

            VoiceChannelCategory = 0,
            DefaultVoiceChannelName = modal.DefaultVoiceChannelNames.Split(",").First().Trim(),
            DefaultPrivateVoiceChannelName = modal.DefaultVoiceChannelNames.Split(",").Last().Trim(),

            VoiceCreationCategoryName = modal.VoiceCreationCategory,
            NewVoiceChannelName = modal.NewVoiceChannel,
            NewPrivateVoiceChannelName = modal.NewPrivateVoiceChannel,

            VoiceChannelCategoryName = modal.VoiceChannelCategory
        });

        await CreateChannelsAsync(id, Data[id]);
        await ServerManager.SaveFeatureDataAsync(id, Feature, Data[id]);
    }
    public async Task AddForServerAsync(ulong id)
    {
        Data.Add(id, (await ServerManager.ReadFeatureDataAsync<VoiceManagerData>(id, Feature)).AsT0);

        _ = CreateChannelsAsync(id, Data[id]);
        await ServerManager.SaveFeatureDataAsync(id, Feature, Data[id]);
    }
    public async Task RemoveForServerAsync(ulong id, bool reset)
    {
        var server = Client.GetGuild(id);
        var data = Data[id];

        _ = server.Channels.First(x => x.Id == data.NewVoiceChannel).DeleteAsync();
        _ = server.Channels.First(x => x.Id == data.NewPrivateVoiceChannel).DeleteAsync();
        _ = server.CategoryChannels.First(x => x.Id == data.VoiceCreationCategory).DeleteAsync();
        _ = server.CategoryChannels.First(x => x.Id == data.VoiceChannelCategory).DeleteAsync();

        _ = Data.Remove(id);

        if (reset) await ServerManager.DeleteFeatureDataAsync(id, Feature);
    }

    public bool IsInCreatedVoiceChannel(SocketGuildUser user) => user.VoiceChannel is not null && user.VoiceChannel.Category.Id == Data[user.Guild.Id].VoiceChannelCategory;

    private async Task CreateChannelsAsync(ulong serverID, VoiceManagerData data)
    {
        data.VoiceCreationCategory = await CreateChannelAsync(serverID, data.VoiceCreationCategoryName, ChannelType.Category);
        data.NewVoiceChannel = await CreateChannelAsync(serverID, data.NewVoiceChannelName, ChannelType.Voice, data.VoiceCreationCategory);
        data.NewPrivateVoiceChannel = await CreateChannelAsync(serverID, data.NewPrivateVoiceChannelName, ChannelType.Voice, data.VoiceCreationCategory);
        data.VoiceChannelCategory = await CreateChannelAsync(serverID, data.VoiceChannelCategoryName, ChannelType.Category);
    }
    private async Task<ulong> CreateChannelAsync(ulong serverID, string name, ChannelType type, ulong categoryId = 0)
    {
        var result = ServerManager.HasChannel(serverID, name, type);

        return result.IsT0
            ? result.AsT0
            : type switch
            {
                ChannelType.Category => (await Client.GetGuild(serverID).CreateCategoryChannelAsync(name)).Id,
                ChannelType.Voice => (await Client.GetGuild(serverID).CreateVoiceChannelAsync(name, x => x.CategoryId = categoryId is not 0 ? categoryId : null)).Id,
                _ => throw new NotImplementedException()
            };
    }

    private async Task CleanUpChannelAsync(SocketVoiceChannel channel)
    {
        await channel.DeleteAsync();

        var permission = channel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow);

        if (permission is IEnumerable<Overwrite> overwrites && overwrites.Any()) await Client.GetGuild(channel.Guild.Id).Roles.First(x => x.Id == overwrites.First().TargetId).DeleteAsync();
    }
    private async Task CreateNewVoiceAsync(SocketGuildUser user)
    {
        var data = Data[user.Guild.Id];
        var channel = await Client.GetGuild(user.Guild.Id).CreateVoiceChannelAsync(data.DefaultVoiceChannelName, x => x.CategoryId = data.VoiceChannelCategory);

        await user.ModifyAsync(x => x.Channel = channel);
    }
    private async Task CreateNewPrivateVoiceAsync(SocketGuildUser user)
    {
        var server = Client.GetGuild(user.Guild.Id);
        var data = Data[user.Guild.Id];

        var role = await server.CreateRoleAsync(data.DefaultPrivateVoiceChannelName, isMentionable: false);
        var channel = await server.CreateVoiceChannelAsync(data.DefaultPrivateVoiceChannelName, x => x.CategoryId = data.VoiceChannelCategory);

        await channel.AddPermissionOverwriteAsync(server.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));

        await user.AddRoleAsync(role);
        await user.ModifyAsync(x => x.Channel = channel);
    }
}