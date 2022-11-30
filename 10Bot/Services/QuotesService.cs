using Newtonsoft.Json;
using System.Diagnostics;
using TenBot.Models;

namespace TenBot.Services;
public sealed class QuotesService
{
    private readonly List<Quote> Quotes;
    private readonly Random Randomizer = new();


    public QuotesService()
    {
        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/quotes.json");
        Quotes = JsonConvert.DeserializeObject<List<Quote>>(json)!;
    }


    public Quote GetQuote(string message) => GetBestMatchingQuote(message);
    public void AddQuote(Quote quote)
    {
        Quotes.Add(quote);
        File.WriteAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/quotes.json",
                          JsonConvert.SerializeObject(Quotes));
    }

    private Quote GetBestMatchingQuote(string message)
    {
        var results = new SimMetrics.Net.Metric.Levenstein().BatchCompareSet(Quotes.Select(x => x.ActualQuote).ToArray(), message).ToList();
        return Quotes[results.IndexOf(results.Max())];
    }
}
