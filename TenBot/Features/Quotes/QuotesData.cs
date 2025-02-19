namespace TenBot.Features.Quotes;
public sealed class QuotesData(Guid id, string quote, string author, string? context = null)
{
    public Guid Id { get; set; } = id;
    public string Quote { get; set; } = quote;
    public string Author { get; set; } = author;
    public string? Context { get; set; } = !context.IsWhiteSpace() ? context : null;


    public QuotesData(string quote, string author, string? context = null) : this(Guid.CreateVersion7(), quote, author, context) { }
}