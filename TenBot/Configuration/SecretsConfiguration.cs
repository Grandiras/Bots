using Microsoft.Extensions.Configuration;

namespace TenBot.Configuration;
public sealed class SecretsConfiguration(ConfigurationManager configuration)
{
    public string Token { get; set; }
#if DEBUG
        = configuration["Token"] ?? throw new Exception("Token missing!");
#else
        = Environment.GetEnvironmentVariable("Token") ?? throw new Exception("Token missing!");
#endif
    public string BetaToken { get; set; }
#if DEBUG
        = configuration["BetaToken"] ?? throw new Exception("Token missing!");
#else
        = Environment.GetEnvironmentVariable("BetaToken") ?? throw new Exception("BetaToken missing!");
#endif
}
