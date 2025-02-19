namespace TenBot.Features.Quotes;
public sealed class Quote(Guid id, string quote, string author, string context = "")
{
    public Guid Id { get; set; } = id;
    public float[] EmbeddedContent { get; set; } = [];
    public string Content { get; set; } = quote;
    public string Author { get; set; } = author;
    public string Context { get; set; } = context;


    public Quote(string quote, string author, string context = "") : this(Guid.CreateVersion7(), quote, author, context) { }
    public Quote() : this(Guid.CreateVersion7(), "", "", "") { }
}