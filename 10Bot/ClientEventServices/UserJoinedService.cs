using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserJoinedService : IClientEventService
{
    private readonly DiscordSocketClient Client;
    private readonly WelcomeMessages WelcomeMessages;
    private readonly ServerService ServerService;


    public UserJoinedService(DiscordSocketClient client, WelcomeMessages welcomeMessages, ServerService serverService)
    {
        Client = client;
        WelcomeMessages = welcomeMessages;
        ServerService = serverService;
    }


    public Task StartAsync()
    {
        Client.UserJoined += OnUserJoined;
        return Task.CompletedTask;
    }

    private async Task OnUserJoined(SocketGuildUser user)
          => await ServerService.GetServer(user.Guild.Id)
                                .DefaultChannel
                                .SendMessageAsync(WelcomeMessages.GetWelcomeMessage(user));
}
