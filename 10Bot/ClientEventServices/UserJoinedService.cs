using Discord.WebSocket;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserJoinedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettingsStorage ServerSettings;
    private readonly WelcomeMessages WelcomeMessages;


    public UserJoinedService(DiscordSocketClient client, DiscordServerSettingsStorage serverSettings, WelcomeMessages welcomeMessages)
    {
        Client = client;
        ServerSettings = serverSettings;
        WelcomeMessages = welcomeMessages;
    }


    public Task StartAsync()
    {
        Client.UserJoined += OnUserJoined;
        return Task.CompletedTask;
    }

    private async Task OnUserJoined(SocketGuildUser user)
        => _ = await Client.GetGuild(ServerSettings.Settings[user.Guild.Id].GuildID)
                           .DefaultChannel
                           .SendMessageAsync(WelcomeMessages.GetWelcomeMessage(user));
}
