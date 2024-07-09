namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesData
{
    public List<string> Messages { get; set; } = [];
    public required ulong ChannelId { get; set; }
}
