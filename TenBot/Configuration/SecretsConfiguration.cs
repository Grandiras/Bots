using Microsoft.Extensions.Configuration;

namespace TenBot.Configuration;
public sealed class SecretsConfiguration(ConfigurationManager configuration)
{
    public string Token { get; set; } = configuration["Secrets:Token"]!;
    public string BetaToken { get; set; } = configuration["Secrets:BetaToken"]!;
}
