using Discord;
using Discord.Interactions;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.AddReactions)]
[Group("quote", "Create memories with this command!")]
public sealed class QuoteCommand : InteractionModuleBase
{
	private readonly QuotesService QuotesService;


	public QuoteCommand(QuotesService quotesService) => QuotesService = quotesService;


    [SlashCommand("create", "Creates a new memory.")]
    public async Task CreateAsync([Summary("quote", "This is the actual quote.")] string quote,
                                  [Summary("author", "The person, who is responsible for this quote.")] string author,
                                  [Summary("display", "Determines, whether this quote should be displayed after creation or not.")] bool display = true,
                                  [Summary("context", "Additional information for this quote.")] string? context = null)
    {
        var quoteObject = new Quote(quote, author, context);
        QuotesService.AddQuote(quoteObject, Context.Guild.Id);

        await RespondAsync("Quote has successfully been created.", ephemeral: true);

        if (!display) return;

        var embed = new EmbedBuilder()
        .WithTitle("New Quote")
        .WithDescription(quote)
        .WithColor(Color.Green)
        .WithFields(new EmbedFieldBuilder()
            .WithName("By")
            .WithValue($"{author}{(context is not null ? $", {context}" : "")}"));

        await RespondAsync(embed: embed.Build());
    }

    [SlashCommand("display", "Prints a random quote.")]
    public async Task DisplayQuoteAsync()
    {
        var quote = QuotesService.GetRandomQuote(Context.Guild.Id);

        var embed = new EmbedBuilder()
            .WithTitle("Quote")
            .WithDescription(quote.ActualQuote)
            .WithColor(Color.Green)
            .WithFields(new EmbedFieldBuilder()
                .WithName("By")
                .WithValue($"{quote.Author}{(quote.Context is not null ? $", {quote.Context}" : "")}"), 
                        new EmbedFieldBuilder()
                .WithName("Wanted by")
                .WithValue(Context.User.Username));

        await RespondAsync(embed: embed.Build());
    }

	[MessageCommand("Search for a quote")]
	public async Task SearchQuoteAsync(IMessage message)
	{
		var quote = QuotesService.GetQuote(message.Content, Context.Guild.Id);

		var embed = new EmbedBuilder()
			.WithTitle("Quote")
			.WithDescription(quote.ActualQuote)
			.WithColor(Color.Green)
			.WithFields(new EmbedFieldBuilder()
				.WithName("By")
				.WithValue($"{quote.Author}{(quote.Context is not null ? $", {quote.Context}" : "")}"),
						new EmbedFieldBuilder()
				.WithName("Responding to")
				.WithValue(message));

        await RespondAsync(embed: embed.Build());
    }
}
