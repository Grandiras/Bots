using Discord.WebSocket;
using Newtonsoft.Json;

namespace TenBot.ClientEventServices;
internal class UserJoinedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;

    private readonly List<string> Messages;
    private readonly Random Randomizer;


    public UserJoinedService(DiscordSocketClient client, DiscordServerSettings serverSettings)
    {
        Client = client;
        ServerSettings = serverSettings;

        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/welcome_messages.json");
        Messages = JsonConvert.DeserializeObject<List<string>>(json)!;
        Randomizer = new();
    }


    public Task StartAsync()
    {
        Client.UserJoined += OnUserJoined;
        return Task.CompletedTask;
    }

    private async Task OnUserJoined(SocketGuildUser user)
        => _ = await Client.GetGuild(ServerSettings.GuildID)
                           .DefaultChannel
                           .SendMessageAsync(Messages[Randomizer.Next(0, Messages.Count - 1)]
                                                                .Replace("[]", user.Username));
}
