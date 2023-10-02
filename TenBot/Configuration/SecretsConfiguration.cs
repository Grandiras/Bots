using Microsoft.Extensions.Configuration;

namespace TenBot.Configuration;
public sealed class SecretsConfiguration
{
    public string Token { get; set; }
    public string BetaToken { get; set; }


    public SecretsConfiguration(ConfigurationManager configuration)
    {
        Token = configuration["Token"]!;
        BetaToken = configuration["BetaToken"]!;
    }
}
