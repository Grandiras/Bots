namespace TenBot.Features.Quotes;
public sealed class QuotesData
{
    public string Quote { get; set; }
    public string Author { get; set; }
    public string? Context { get; set; }


    public QuotesData(string quote, string author, string? context = null)
    {
        Quote = quote;
        Author = author;
        Context = context;
    }
}