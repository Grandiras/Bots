using Discord;

namespace TenBot.Models;
public sealed class Poll
{
	public string Title { get; init; }
	public string Description { get; init; }
	public TimeSpan Duration { get; init; }
	public List<PollOption> Options { get; init; }
	public IUserMessage Message { get; init; }

	public List<ulong> UsersVoted { get; } = new();


	public Poll(string title, string description, TimeSpan duration, List<PollOption> options, IUserMessage message)
	{
		Title = title;
		Description = description;
		Duration = duration;
		Options = options;
		Message = message;
	}
}

public class PollOption
{
	public string Name { get; }
	public uint Count { get; set; }

	public PollOption(string name) => Name = name;
}
