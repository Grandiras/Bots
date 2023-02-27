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
        var task = Task.Factory.StartNew(() => PollManager(poll, message));
    }
    public void CastVote(string pollName, uint optionNumber, ulong userId)
    {
        var poll = Polls.First(x => x.Poll.Title == pollName.Replace("-", " "));

        if (optionNumber == 1) poll.Poll.Option1.Count++;
        else if (optionNumber == 2) poll.Poll.Option2.Count++;
        else if (optionNumber == 3) poll.Poll.Option3!.Count++;
        else if (optionNumber == 4) poll.Poll.Option4!.Count++;
        else if (optionNumber == 5) poll.Poll.Option5!.Count++;
        else if (optionNumber == 6) poll.Poll.Option6!.Count++;

        poll.VoteCount++;
        poll.UsersVoted.Add(userId);
    }
    public bool HasUserVoted(string pollName, ulong userId) => Polls.First(x => x.Poll.Title == pollName.Replace("-", " ")).UsersVoted.Contains(userId);
    public PollOption GetPollOption(string pollName, uint optionNumber)
    {
        var poll = Polls.First(x => x.Poll.Title == pollName.Replace("-", " "));

        if (optionNumber == 1) return poll.Poll.Option1;
        else if (optionNumber == 2) return poll.Poll.Option2;
        else if (optionNumber == 3) return poll.Poll.Option3!;
        else if (optionNumber == 4) return poll.Poll.Option4!;
        else if (optionNumber == 5) return poll.Poll.Option5!;
        else if (optionNumber == 6) return poll.Poll.Option6!;

        throw new NotImplementedException();
    }
    public bool PollExists(string pollName) => Polls.Any(x => x.Poll.Title == pollName);

    private PollData GetPollData(string pollName) => Polls.First(x => x.Poll.Title == pollName.Replace("-", " "));

    private async Task PollManager(Poll poll, IUserMessage message)
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

            _ = components.WithButton(poll.Option1.Name, $"{poll.Title.Replace(" ", "-")}_option1");
            _ = components.WithButton(poll.Option2.Name, $"{poll.Title.Replace(" ", "-")}_option2");
            if (poll.Option3 is not null) _ = components.WithButton(poll.Option3.Name, $"{poll.Title.Replace(" ", "-")}_option3");
            if (poll.Option4 is not null) _ = components.WithButton(poll.Option4.Name, $"{poll.Title.Replace(" ", "-")}_option4");
            if (poll.Option5 is not null) _ = components.WithButton(poll.Option5.Name, $"{poll.Title.Replace(" ", "-")}_option5");
            if (poll.Option6 is not null) _ = components.WithButton(poll.Option6.Name, $"{poll.Title.Replace(" ", "-")}_option6");

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
