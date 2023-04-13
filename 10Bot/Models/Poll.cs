using Discord;

namespace TenBot.Models;
public sealed record Poll(string Title, string Description, uint Duration, params PollOption[] Options);

public sealed class PollOption
{

    public string Name { get; init; }
    public uint Count { get; set; }

    public PollOption(string name, uint count = 0)
    {
        Name = name;
        Count = count;
    }
}

public sealed class PollData
{
    public Poll Poll { get; }
    public IUserMessage Message { get; }
    public long VoteCount => Poll.Options.Sum(x => x.Count);
    public List<ulong> UsersVoted { get; } = new();

    public PollData(Poll poll, IUserMessage message)
    {
        Poll = poll;
        Message = message;
    }
}
