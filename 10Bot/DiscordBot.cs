using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using TenBot.ClientEventServices;
using TenBot.Services;

namespace TenBot;
internal sealed class DiscordBot
{
	private readonly IHost Host;


	public DiscordBot() => Host = Microsoft.Extensions.Hosting.Host
		.CreateDefaultBuilder()
		.ConfigureLogging((logger) => logger.AddConsole())
		.ConfigureServices((context, services) => _ = services

			.AddSingleton(new TenBotSettings
			{
				IsBeta = true,
				RootPath = Directory.GetCurrentDirectory().Split("/bin")[0] + "/Data"
			})

			.AddSingleton(new DiscordSocketConfig())
			.AddSingleton<DiscordSocketClient>()

			.AddSingleton(new InteractionServiceConfig())
			.AddSingleton<InteractionService>()
			.AddSingleton<InteractionHandler>()

			.Scan(scan => scan
				.FromCallingAssembly()
				.AddClasses(classes => classes
					.AssignableTo<IService>())
					.AsSelf()
					.WithSingletonLifetime()))

		.Build();


	public async Task RunAsync()
	{
		var client = Host.Services.GetRequiredService<DiscordSocketClient>();
		var settings = Host.Services.GetRequiredService<TenBotSettings>();

		client.Log += async (msg) =>
		{
			Console.WriteLine(msg.ToString());
			await Task.CompletedTask;
		};

		await Host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
		foreach (var service in Host.Services.GetServices<IClientEventService>()) await service.StartAsync();

		await client.LoginAsync(TokenType.Bot, settings.Configuration.Token);
		await client.StartAsync();

		await Task.Delay(-1); // infinite timeout
	}
}
