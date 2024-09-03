using Microsoft.Extensions.Logging;
using TenBot.Configuration;

namespace TenBot.Services;
public sealed class BotConfigurator : IService
{
    private readonly BotConfiguration BotConfiguration;
    private readonly SecretsConfiguration SecretsConfiguration;
    private readonly ILogger<BotConfigurator> Logger;


    public BotConfigurator(BotConfiguration botConfiguration, SecretsConfiguration secretsConfiguration, ILogger<BotConfigurator> logger)
    {
        BotConfiguration = botConfiguration;
        SecretsConfiguration = secretsConfiguration;
        Logger = logger;

        Logger.LogInformation("Bot is running in {} mode.", BotConfiguration.IsBeta ? "beta" : "production");
    }


    public string GetToken() => BotConfiguration.IsBeta ? SecretsConfiguration.BetaToken : SecretsConfiguration.Token;
}
