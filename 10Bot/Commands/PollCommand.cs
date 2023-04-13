using Discord;
using Discord.Interactions;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Commands;
//[Group("poll", "Do you want to know the opinions of your fellow server members? Use this command to create polls!")]
public sealed class PollCommand : InteractionModuleBase
{
    private readonly PollService PollService;


    public PollCommand(PollService pollService) => PollService = pollService;


    // TODO - WIP
    //[SlashCommand("create", "Creates a new poll.")]
    public async Task CreateAsync([Summary("title", "Give this poll a name!")] string title,
                                  [Summary("description", "Further describe the poll.")] string description,
                                  [Summary("duration", "Define, how long the poll should be open (in seconds)!")] uint duration,
                                  [Summary("option_1", "This is the first option to vote for.")] string option1,
                                  [Summary("option_2", "This is the first option to vote for.")] string option2,
                                  [Summary("option_3", "This is the first option to vote for.")] string? option3 = null,
                                  [Summary("option_4", "This is the first option to vote for.")] string? option4 = null,
                                  [Summary("option_5", "This is the first option to vote for.")] string? option5 = null,
                                  [Summary("option_6", "This is the first option to vote for.")] string? option6 = null)
    {
        if (PollService.PollExists(title))
        {
            await RespondAsync("A poll with this name currently exists!", ephemeral: true);
            return;
        }

        await RespondAsync($"Poll '{title}' was successfully created.", ephemeral: true);

        var embed = new EmbedBuilder()
            .WithTitle(title)
            .WithDescription(description)
            .WithColor(Color.Blue)
            .AddField(new EmbedFieldBuilder()
                .WithName("Duration")
                .WithValue($"{duration} seconds"));

        var pollOptions = new List<PollOption>
        {
            new(option1),
            new(option2)
        };
        if (option3 is not null) pollOptions.Add(new(option3));
        if (option4 is not null) pollOptions.Add(new(option4));
        if (option5 is not null) pollOptions.Add(new(option5));
        if (option6 is not null) pollOptions.Add(new(option6));

        var poll = new Poll(title, description, duration, pollOptions.ToArray());

        var components = new ComponentBuilder();
        for (var i = 0; i < poll.Options.Length; i++) _ = components.WithButton(poll.Options[i].Name, $"{title.Replace(" ", "-")}_option{i}");

        var message = await Context.Channel.SendMessageAsync(embed: embed.Build(), components: components.Build());

        PollService.CreatePoll(poll, message);
    }

    //[ComponentInteraction("*_option*", true)]
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
