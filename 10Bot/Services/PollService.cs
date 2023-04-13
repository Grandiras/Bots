using Discord;
using System.Diagnostics;
using TenBot.Models;

namespace TenBot.Services;
public class PollService : IService
{
    private readonly List<PollData> Polls = new();


    public void CreatePoll(Poll poll, IUserMessage message)
    {
        Polls.Add(new(poll, message));
        var task = Task.Factory.StartNew(() => ManagePoll(poll, message));
    }
    public void CastVote(string pollName, uint optionNumber, ulong userId)
    {
        var poll = Polls.First(x => x.Poll.Title == pollName.Replace("-", " "));

        poll.Poll.Options[optionNumber].Count++;
        poll.UsersVoted.Add(userId);
    }
    public bool HasUserVoted(string pollName, ulong userId) => Polls.First(x => x.Poll.Title == pollName.Replace("-", " ")).UsersVoted.Contains(userId);
	public PollOption GetPollOption(string pollName, uint optionNumber) => Polls.First(x => x.Poll.Title == pollName.Replace("-", " ")).Poll.Options[optionNumber];
	public bool PollExists(string pollName) => Polls.Any(x => x.Poll.Title == pollName);

    private PollData GetPollData(string pollName) => Polls.First(x => x.Poll.Title == pollName.Replace("-", " "));

    private async Task ManagePoll(Poll poll, IUserMessage message)
    {
        var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        var watch = new Stopwatch();

        watch.Start();

        while (watch.ElapsedMilliseconds < poll.Duration * 1000 && await timer.WaitForNextTickAsync())
        {
            var embed = new EmbedBuilder()
                .WithTitle(poll.Title)
                .WithDescription(poll.Description)
                .WithColor(Color.Blue)
                .AddField(new EmbedFieldBuilder()
                    .WithName("Duration")
                    .WithValue($"{poll.Duration - (int)(watch.ElapsedMilliseconds / 1000)} seconds"));

            var components = new ComponentBuilder();
            for (var i = 0; i < poll.Options.Length; i++) _ = components.WithButton(poll.Options[i].Name, $"{poll.Title.Replace(" ", "-")}_option{i}");

            await message.ModifyAsync(x =>
            {
                x.Embed = embed.Build();
                x.Components = components.Build();
            });
        }

        var pollData = GetPollData(poll.Title);

        var embed2 = new EmbedBuilder()
            .WithTitle(poll.Title)
            .WithDescription(poll.Description)
            .WithColor(Color.Red)
            .AddField(new EmbedFieldBuilder()
                .WithName("Users voted")
                .WithValue(pollData.UsersVoted.Count)); ;

        await message.ModifyAsync(x =>
        {
            x.Embed = embed2.Build();
            x.Components = null;
        });

        _ = Polls.Remove(Polls.First(x => x.Poll.Title == poll.Title));
    }
}
