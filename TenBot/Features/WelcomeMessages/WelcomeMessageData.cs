namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessageData
{
    public List<string> Messages { get; set; } = new();
    public required ulong ChannelId { get; set; }
}
