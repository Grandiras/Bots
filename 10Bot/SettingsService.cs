namespace TenBot;
public sealed class SettingsService
{
    public required string RootDirectory { get; init; }
    public required bool IsBeta { get; init; }
}
