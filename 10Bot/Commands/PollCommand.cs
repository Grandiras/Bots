using Discord;
using Discord.Interactions;
using System.Globalization;
using TenBot.Models;
using TenBot.Services;

namespace TenBot_Legacy.Commands;
[Group("poll", "Do you want to know the opinions of your fellow server members? Use this command to create polls!")]
public sealed class PollCommand : InteractionModuleBase
{
    private readonly PollService PollService;


    public PollCommand(PollService pollService) => PollService = pollService;


    [SlashCommand("create", "Creates a new poll.")]
    public async Task CreateAsync([Summary("title", "Give this poll a name!")] string title,
                                  [Summary("description", "Further describe the poll.")] string description,
                                  [Summary("duration", "Define, how long the poll should be open (in seconds).")] int duration,
                                  [Summary("options", "Provide all the vote options you want (separate by comma).")] string options)
    {
        if (PollService.PollExists(title))
        {
            await RespondAsync("A poll with this name already exsits at this moment!", ephemeral: true);
            return;
        }

        await RespondAsync($"Poll '{title}' was successfully created.", ephemeral: true);

        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(Color.Blue)
            .AddField(new EmbedFieldBuilder()
                .WithName("Will close at")
                .WithValue($"{DateTime.Now.AddSeconds(duration).ToString(CultureInfo.InvariantCulture)}"));

        var allOptions = options.Split(',').Select(x => x.Trim()).ToList();

        var components = new ComponentBuilder();
        foreach (var option in allOptions) _ = components.WithButton(option, $"{title.Replace(" ", "-")}_option{allOptions.IndexOf(option)}");
        var message = await Context.Channel.SendMessageAsync(embed: embed.Build(), components: components.Build());

        var poll = new Poll(title, description, new(0, 0, duration), allOptions.Select(x => new PollOption(x)).ToList(), message);
        PollService.CreatePoll(poll);
    }

    [ComponentInteraction("*_option*", true)]
    public async Task VoteCastedAsync(string pollName, uint optionNumber)
    {
        if (PollService.HasUserVoted(pollName, Context.User.Id))
        {
            await RespondAsync("You have already voted in this poll!", ephemeral: true);
            return;
        }

        var option = Convert.ToUInt32(optionNumber);
        PollService.CastVote(pollName, option, Context.User.Id);

        await RespondAsync($"You have voted for '{PollService.GetPollOption(pollName, option).Name}'.", ephemeral: true);
    }
}
