using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TenBot;
using TenBot.ClientEventServices;
using TenBot.Services;

var host = Host
		.CreateDefaultBuilder()
		.ConfigureServices((context, services) => _ = services

			.AddSingleton(new TenBotSettings
			{
				IsBeta = true,
				RootPath = Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data"
			})

			.AddSingleton(new DiscordSocketConfig())
			.AddSingleton<DiscordSocketClient>()

			.AddSingleton(new InteractionServiceConfig())
			.AddSingleton<InteractionService>()
			.AddSingleton<InteractionHandler>()

			.Scan(scan => scan
				.FromCallingAssembly()
				.AddClasses(classes => classes.AssignableTo<IClientEventService>())
					.As<IClientEventService>()
					.WithSingletonLifetime()
				.AddClasses(classes => classes.AssignableTo<IService>())
					.AsSelf()
					.WithSingletonLifetime()))

		.Build();

var client = host.Services.GetRequiredService<DiscordSocketClient>();
var settings = host.Services.GetRequiredService<TenBotSettings>();

client.Log += async (msg) =>
{
	Console.WriteLine(msg.ToString());
	await Task.CompletedTask;
};

await host.Services.GetRequiredService<InteractionHandler>().InitializeAsync();
foreach (var service in host.Services.GetServices<IClientEventService>()) await service.StartAsync();

await client.LoginAsync(TokenType.Bot, settings.Configuration.Token);
await client.StartAsync();

await Task.Delay(-1); // infinite timeout