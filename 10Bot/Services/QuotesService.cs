using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class QuotesService
{
    private readonly Dictionary<ulong, List<Quote>> Quotes = new();


    public QuotesService(DiscordServerSettingsStorage serverSettings)
    {
        foreach (var server in serverSettings.Settings.Keys)
            Quotes.Add(server,
                       JsonConvert.DeserializeObject<List<Quote>>(File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + $"/Data/Servers/{server}/quotes.json"))!);
    }


    public Quote GetQuote(string message, ulong guildID)
    {
        var quotes = Quotes[guildID];
        var results = new SimMetrics.Net.Metric.Levenstein().BatchCompareSet(quotes.Select(x => x.ActualQuote).ToArray(), message).ToList();
        return quotes[results.IndexOf(results.Max())];
    }
    public void AddQuote(Quote quote, ulong guildID)
    {
        var quotes = Quotes[guildID];
        quotes.Add(quote);
        File.WriteAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + $"/Data/Servers/{guildID}/quotes.json",
                          JsonConvert.SerializeObject(quotes));
    }
}
