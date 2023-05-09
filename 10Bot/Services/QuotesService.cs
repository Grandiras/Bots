using TenBot.Models;

namespace TenBot.Services;
public sealed class QuotesService : IService, IDisposable
{
	private const string FILE_NAME = "quotes.json";

	private readonly ServerSettings ServerSettings;

	private readonly Dictionary<ulong, List<Quote>> Quotes = new();

    private readonly SettingsService Settings;

	public QuotesService(ServerSettings serverSettings)
	{
		ServerSettings = serverSettings;

		Quotes = ServerSettings.GetAllServerConfiguration<List<Quote>>(FILE_NAME);
	}


	public Quote GetQuote(string message, ulong serverID)
	{
		var quotes = Quotes[serverID];
		var results = new SimMetrics.Net.Metric.Levenstein().BatchCompareSet(quotes.Select(x => x.ActualQuote).ToArray(), message).ToList();
		return quotes[results.IndexOf(results.Max())];
	}
	public Quote GetRandomQuote(ulong guildID) => Quotes[serverID][Random.Shared.Next(0, Quotes[serverID].Count)];

	public void AddQuote(Quote quote, ulong serverID)
	{
		var quotes = Quotes[serverID];
		quotes.Add(quote);

		ServerSettings.SaveServerConfiguration(serverID, FILE_NAME, quotes);
	}

	public void Dispose() => ServerSettings.SaveAllServerConfiguration(FILE_NAME, Quotes);
}
