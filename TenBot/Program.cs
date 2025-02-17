using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Configuration;
using TenBot.Features;
using TenBot.Helpers;
using TenBot.Services;
using TenBot.StandardFeatures;

var builder = WebApplication.CreateBuilder();

builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

builder.Services
    .AddSingleton(new SecretsConfiguration(builder.Configuration))
    .AddSingleton(builder.Configuration.GetSection("Bot").Get<BotConfiguration>()!);

builder.Services
    .AddSingleton<DiscordSocketClient>()
    .AddSingleton(new DiscordSocketConfig() { GatewayIntents = GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent })
    .AddSingleton<ServerInteractionHandler>()
    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), x.GetRequiredService<InteractionServiceConfig>()))
    .AddSingleton(new InteractionServiceConfig() { UseCompiledLambda = true });

builder.Services.Scan(scan => scan.FromCallingAssembly()
    .AddClasses(classes => classes.AssignableTo<IService>())
        .AsSelf()
        .WithSingletonLifetime()
    .AddClasses(classes => classes.AssignableTo<IStandardFeature>())
        .AsSelf()
        .WithSingletonLifetime()
    .AddClasses(classes => classes.AssignableTo<IFeature>())
        .AsSelf()
        .WithSingletonLifetime());

var app = builder.Build();

app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) _ = app.MapOpenApi();

var client = app.Services.GetRequiredService<DiscordSocketClient>();
var configurator = app.Services.GetRequiredService<BotConfigurator>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

client.Log += async (msg) =>
{
    logger.Log(msg.Severity.ToLogLevel(), "{}", msg.ToString());
    await Task.CompletedTask;
};
client.Ready += async () =>
{
    foreach (var service in app.Services.GetAllServicesWith<IMustPostInitialize>()) await service.PostInitializeAsync();
};

foreach (var service in app.Services.GetAllServicesWith<IMustInitialize>()) _ = service.InitializeAsync();

await client.LoginAsync(TokenType.Bot, configurator.GetToken());
await client.StartAsync();

await client.SetCustomStatusAsync("Use /help for more information!");

app.MapDefaultEndpoints();

app.Run();