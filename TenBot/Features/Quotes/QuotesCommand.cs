using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.Quotes;
[Group("quotes", "Create memories with this command."), DefaultMemberPermissions(GuildPermission.SendMessages)]
public sealed class QuotesCommand(QuotesService QuotesService) : InteractionModuleBase<ServerInteractionContext>
{
    [SlashCommand("create", "Creates a new memory.")]
    public async Task CreateAsync([Summary("quote", "This is the actual quote.")] string quote,
                                  [Summary("author", "The person, who is responsible for this quote.")] string author,
                                  [Summary("display", "Determines, whether this quote should be displayed after creation or not.")] bool display = true,
                                  [Summary("context", "Additional information for this quote.")] string? context = null)
    {
        _ = QuotesService.AddQuote(new(quote, author, context), Context.ServerID);

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("New Quote")
            .WithDescription(quote)
            .WithColor(QuotesService.Feature.Color)
            .WithFields(
                new EmbedFieldBuilder()
                .WithName("By")
                .WithValue($"{author}{(context is not null ? $", {context}" : "")}"))
            .Build(), ephemeral: !display);
    }

    [SlashCommand("delete", "Deletes a quote.")]
    public async Task DeleteAsync([Summary("quote", "The quote to delete."), Autocomplete(typeof(QuotesAutoCompleteHandler))] string quote)
    {
        if (quote is null || (await QuotesService.GetQuote(Guid.Parse(quote), Context.ServerID)).IsT1)
        {
            await RespondAsync("Quote not found.", ephemeral: true);
            return;
        }

        await QuotesService.RemoveQuote(Guid.Parse(quote), Context.ServerID);
        await RespondAsync("Quote has successfully been deleted.", ephemeral: true);
    }

    [SlashCommand("display", "Prints a random quote.")]
    public async Task DisplayAsync([Summary("display", "Determines, whether this quote should be displayed to everyone or not.")] bool display = true)
    {
        var quote = await QuotesService.GetRandomQuote(Context.Guild.Id);

        if (quote.IsT1)
        {
            await RespondAsync("No quotes found.", ephemeral: true);
            return;
        }

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("Quote")
            .WithDescription(quote.AsT0.Quote)
            .WithColor(QuotesService.Feature.Color)
            .WithFields(
                new EmbedFieldBuilder()
                .WithName("By")
                .WithValue($"{quote.AsT0.Author}{(quote.AsT0.Context is not null ? $", {quote.AsT0.Context}" : "")}"),
                new EmbedFieldBuilder()
                .WithName("Requested by")
                .WithValue(Context.User.Mention))
            .Build(), ephemeral: !display);
    }

    [SlashCommand("list", "Lists all available quotes.")]
    public async Task ListAsync()
        => await RespondAsync(embed: new EmbedBuilder()
        .WithTitle("Quotes")
        .WithColor(QuotesService.Feature.Color)
        .WithFields((await QuotesService.GetQuotes(Context.Guild.Id)).Select(x => new EmbedFieldBuilder()
            .WithName(x.Quote)
            .WithValue(x.Author + (x.Context is not null ? $", {x.Context}" : ""))))
        .Build(), ephemeral: true);

    [MessageCommand("Search for a quote")]
    public async Task SearchAsync([Summary("message", "The message to search for.")] IMessage message)
    {
        var quote = await QuotesService.GetMatchingQuote(message.Content, Context.Guild.Id);

        if (quote.IsT1)
        {
            await RespondAsync("No matching quote found!", ephemeral: true);
            return;
        }

        await RespondAsync(embed: new EmbedBuilder()
            .WithTitle("Quote")
            .WithDescription(quote.AsT0.Quote)
            .WithColor(QuotesService.Feature.Color)
            .WithFields(
                new EmbedFieldBuilder()
                .WithName("By")
                .WithValue($"{quote.AsT0.Author}{(quote.AsT0.Context is not null ? $", {quote.AsT0.Context}" : "")}"),
                new EmbedFieldBuilder()
                .WithName("Requested by")
                .WithValue(Context.User.Mention))
            .Build());
    }
}