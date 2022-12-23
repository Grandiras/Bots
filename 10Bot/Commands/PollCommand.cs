using Discord;
using Discord.Interactions;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Commands;
[Group("poll", "Do you want to know the opinions of your fellow server members? Use this command to create polls!")]
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
            await RespondAsync("A poll with this name currently exsits!", ephemeral: true);
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

        var components = new ComponentBuilder();

        _ = components.WithButton(option1, $"{title.Replace(" ", "-")}_option1");
        _ = components.WithButton(option2, $"{title.Replace(" ", "-")}_option2");
        if (option3 is not null) _ = components.WithButton(option3, $"{title.Replace(" ", "-")}_option3");
        if (option4 is not null) _ = components.WithButton(option3, $"{title.Replace(" ", "-")}_option4");
        if (option5 is not null) _ = components.WithButton(option3, $"{title.Replace(" ", "-")}_option5");
        if (option6 is not null) _ = components.WithButton(option3, $"{title.Replace(" ", "-")}_option6");

        var message = await Context.Channel.SendMessageAsync(embed: embed.Build(), components: components.Build());

        var poll = new Poll(title, description, duration, new PollOption(option1), new PollOption(option2),
                            option3 is not null ? new PollOption(option3) : null,
                            option4 is not null ? new PollOption(option4) : null,
                            option5 is not null ? new PollOption(option5) : null,
                            option6 is not null ? new PollOption(option6) : null);

        PollService.CreatePoll(poll, message);
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
