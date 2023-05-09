﻿using Discord.WebSocket;
using TenBot.Services;

namespace TenBot.ClientEventServices;
internal sealed class UserJoinedService : IClientEventService
{
	private readonly DiscordSocketClient Client;
	private readonly WelcomeMessages WelcomeMessages;


	public UserJoinedService(DiscordSocketClient client, WelcomeMessages welcomeMessages)
	{
		Client = client;
		WelcomeMessages = welcomeMessages;
	}


	public Task StartAsync()
	{
		Client.UserJoined += OnUserJoined;
		return Task.CompletedTask;
	}

	private async Task OnUserJoined(SocketGuildUser user)
		=> _ = await Client.GetGuild(user.Guild.Id).DefaultChannel.SendMessageAsync(WelcomeMessages.GetWelcomeMessage(user));
}
