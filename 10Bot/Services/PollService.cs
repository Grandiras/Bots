using Discord;
using System.Text;
using TenBot.Models;

namespace TenBot.Services;
public class PollService : IService
{
	private readonly List<Poll> Polls = new();


	public void CreatePoll(Poll poll)
	{
		Polls.Add(poll);
		var task = Task.Factory.StartNew(() => ManagePoll(poll));
	}
	public void CastVote(string pollName, uint optionNumber, ulong userId)
	{
		var poll = Polls.First(x => x.Title == pollName.Replace("-", " "));

		poll.Options[(int)optionNumber].Count++;
		poll.UsersVoted.Add(userId);
	}
	public bool HasUserVoted(string pollName, ulong userId) => Polls.First(x => x.Title == pollName.Replace("-", " ")).UsersVoted.Contains(userId);
	public PollOption GetPollOption(string pollName, uint optionNumber) => Polls.First(x => x.Title == pollName.Replace("-", " ")).Options[(int)optionNumber];
	public bool PollExists(string pollName) => Polls.Any(x => x.Title == pollName);

	private async Task ManagePoll(Poll poll)
	{
		await Task.Delay(poll.Duration);

		if (poll.UsersVoted.Count is 0)
		{
			var embed3 = new EmbedBuilder()
				.WithTitle(poll.Title)
				.WithDescription(poll.Description)
				.WithColor(Color.Red)
				.AddField(new EmbedFieldBuilder()
					.WithName("Users voted")
					.WithValue("No one voted!")
					.WithIsInline(true));

			await poll.Message.ModifyAsync(x =>
			{
				x.Embed = embed3.Build();
				x.Components = null;
			});

			_ = Polls.Remove(Polls.First(x => x.Title == poll.Title));

			return;
		}

		var optionsRanking = poll.Options.OrderByDescending(x => x.Count);

		var embed2 = new EmbedBuilder()
			.WithTitle(poll.Title)
			.WithDescription(poll.Description)
			.WithColor(Color.Red)
			.AddField(new EmbedFieldBuilder()
				.WithName("Users voted")
				.WithValue(poll.UsersVoted.Count)
				.WithIsInline(true))
			.AddField(new EmbedFieldBuilder()
				.WithName("Option chosen")
				.WithValue(optionsRanking.First().Name)
				.WithIsInline(true));

		var results = new StringBuilder();
		foreach (var option in optionsRanking) _ = results.Append($"{(option.Count is not 0 ? new string('▰', (int)(option.Count / (double)poll.UsersVoted.Count * 20)) : "").PadRight(20, '▱')} {(option.Count is not 0 ? (int)(option.Count / (double)poll.UsersVoted.Count * 100) : 0).ToString().PadLeft(3, '0')}% {option.Name}\n");

		_ = embed2.AddField(new EmbedFieldBuilder()
			.WithName("Distribution")
			.WithValue(results.ToString()));

		await poll.Message.ModifyAsync(x =>
		{
			x.Embed = embed2.Build();
			x.Components = null;
		});

		_ = Polls.Remove(Polls.First(x => x.Title == poll.Title));
	}
}
