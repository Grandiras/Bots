namespace TenBot.Features.Quotes;
public sealed class QuotesData(string quote, string author, string? context = null)
{
    public string Quote { get; set; } = quote;
    public string Author { get; set; } = author;
    public string? Context { get; set; } = context;
}