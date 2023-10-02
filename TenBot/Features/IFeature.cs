using TenBot.Models;

namespace TenBot.Features;
public interface IFeature
{
    ServerFeature Feature { get; }

    Task AddForServerAsync(ulong id);
    Task RemoveForServerAsync(ulong serverID, bool reset);
}
