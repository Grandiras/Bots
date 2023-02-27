using Discord;

namespace TenBot.Models;
public sealed record Poll(string Title, string Description, uint Duration,
                          PollOption Option1, PollOption Option2, PollOption? Option3 = null, PollOption? Option4 = null, PollOption? Option5 = null, PollOption? Option6 = null);

public sealed record PollOption(string Name, uint Count = 0);

public sealed class PollData
{
    public Poll Poll { get; }
    public IUserMessage Message { get; }
    public uint VoteCount { get; set; }
    public List<ulong> UsersVoted { get; } = new();

    public PollData(Poll poll, IUserMessage message)
    {
        Poll = poll;
        Message = message;
    }
}
