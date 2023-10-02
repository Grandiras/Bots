namespace TenBot.Models;
public sealed class Server
{
    public required string Name { get; init; }
    public required ulong Id { get; init; }
    public required bool IsBeta { get; init; }

    public List<string> Features { get; init; } = new();
}
