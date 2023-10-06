using Discord;
using OneOf;
using OneOf.Types;
using SimMetrics.Net.Metric;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.Quotes;
public sealed class QuotesService : IFeature
{
    private readonly FeatureService FeatureService;
    private readonly ServerService ServerService;

    private readonly Dictionary<ulong, List<QuotesData>> Quotes = new();

    public ServerFeature Feature => new()
    {
        Name = "Quotes",
        Description = "Create memories with this feature.",
        Color = Color.Green,
        IsStandard = false,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureService.GetModuleInfo<QuotesCommand>
    };


    public QuotesService(FeatureService featureService, ServerService serverService)
    {
        FeatureService = featureService;
        ServerService = serverService;

        Quotes = ServerService.ReadConcurrentFeatureDataWithKeysAsync<List<QuotesData>>(Feature).Result.AsT0;
    }


    public async Task AddForServerAsync(ulong id)
    {
        Quotes.Add(id, (await ServerService.ReadFeatureDataAsync<List<QuotesData>>(id, Feature)).Match(some => some, none => new()));
        await ServerService.SaveFeatureDataAsync(id, Feature, Quotes[id]);
    }
    public async Task RemoveForServerAsync(ulong serverID, bool reset)
    {
        _ = Quotes.Remove(serverID);
        if (reset) await ServerService.DeleteFeatureDataAsync(serverID, Feature);
    }

    public OneOf<QuotesData, NotFound> GetQuote(string quote, ulong serverID)
        => Quotes[serverID].FirstOrDefault(x => x.Quote == quote) is QuotesData data and not null
            ? data
            : new NotFound();
    public List<QuotesData> GetQuotes(ulong serverID) => Quotes[serverID];
    public OneOf<QuotesData, None> GetRandomQuote(ulong serverID)
        => Quotes[serverID].Count is not 0
            ? Quotes[serverID][Random.Shared.Next(0, Quotes[serverID].Count)]
            : new None();
    public OneOf<QuotesData, None> GetMatchingQuote(string message, ulong serverID)
    {
        if (Quotes[serverID].Count is 0) return new None();

        var results = new Levenstein().BatchCompareSet(Quotes[serverID].Select(x => x.Quote).ToArray(), message).ToList();
        return results.Max() is >= 0.5 ? Quotes[serverID][results.IndexOf(results.Max())] : new None();
    } // TODO fine tuning

    public void AddQuote(QuotesData quote, ulong serverID)
    {
        Quotes[serverID].Add(quote);
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Quotes[serverID]);
    }
    public void RemoveQuote(string quote, ulong serverID)
    {
        _ = Quotes[serverID].Remove(Quotes[serverID].First(x => x.Quote == quote));
        _ = ServerService.SaveFeatureDataAsync(serverID, Feature, Quotes[serverID]);
    }
}
