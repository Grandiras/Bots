namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesData
{
    public List<string> Messages { get; set; } = new();
    public required ulong ChannelId { get; set; }
}
