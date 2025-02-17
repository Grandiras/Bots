using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Configuration;
using TenBot.Features;
using TenBot.Helpers;
using TenBot.Services;
using TenBot.StandardFeatures;

var builder = WebApplication.CreateBuilder();

// Add defaults
builder.AddServiceDefaults();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// Add secrets configuration
builder.Services
    .AddSingleton(new SecretsConfiguration(builder.Configuration))
    .AddSingleton(builder.Configuration.GetSection("Bot").Get<BotConfiguration>()!);

// Add Discord configuration and services
builder.Services
    .AddSingleton<DiscordSocketClient>()
    .AddSingleton(new DiscordSocketConfig() { GatewayIntents = (GatewayIntents.AllUnprivileged | GatewayIntents.MessageContent) & ~GatewayIntents.GuildScheduledEvents & ~GatewayIntents.GuildInvites })
    .AddSingleton<ServerInteractionHandler>()
    .AddSingleton(x => new InteractionService(x.GetRequiredService<DiscordSocketClient>(), x.GetRequiredService<InteractionServiceConfig>()))
    .AddSingleton(new InteractionServiceConfig() { UseCompiledLambda = true });

// Add features and services flagged with the appropriate interfaces
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

// Add exception handling and OpenAPI documentation
app.UseExceptionHandler();
if (app.Environment.IsDevelopment()) _ = app.MapOpenApi();

// Add logging and Discord event handlers
var client = app.Services.GetRequiredService<DiscordSocketClient>();
var configurator = app.Services.GetRequiredService<BotConfigurator>();
var logger = app.Services.GetRequiredService<ILogger<Program>>();

client.Log += async (msg) =>
{
    logger.Log(msg.Severity.ToLogLevel(), "{}", msg.ToString(prependTimestamp: false));
    await Task.CompletedTask;
};
client.Ready += async () =>
{
    foreach (var service in app.Services.GetAllServicesWith<IMustPostInitialize>()) await service.PostInitializeAsync();
};

// Initialize services that require it
foreach (var service in app.Services.GetAllServicesWith<IMustInitialize>()) _ = service.InitializeAsync();

// Start the bot
await client.LoginAsync(TokenType.Bot, configurator.GetToken());
await client.StartAsync();

// Set the bot's status
await client.SetCustomStatusAsync("Use /help for more information!");

// Add the default endpoints and run the application
app.MapDefaultEndpoints();
app.Run();