using Discord;
using Microsoft.Extensions.Logging;

namespace TenBot.Helpers;
public static class LogSeverityExtensions
{
    public static LogLevel ToLogLevel(this LogSeverity severity) => severity switch
    {
        LogSeverity.Critical => LogLevel.Critical,
        LogSeverity.Error => LogLevel.Error,
        LogSeverity.Warning => LogLevel.Warning,
        LogSeverity.Info => LogLevel.Information,
        LogSeverity.Verbose => LogLevel.Trace,
        LogSeverity.Debug => LogLevel.Debug,
        _ => throw new ArgumentException($"Unknown {nameof(LogSeverity)}: {severity}"),
    };
}
