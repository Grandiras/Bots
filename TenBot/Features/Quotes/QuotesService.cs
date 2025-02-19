using AllMiniLmL6V2Sharp;
using Discord;
using OneOf;
using OneOf.Types;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.Quotes;
public sealed class QuotesService(FeatureService FeatureService, ServerService ServerService, ILogger<QuotesService> Logger, VectorDatabaseService VectorDatabaseService) : IFeature, IMustInitialize
{
    private VectorDatabaseCollection<Quote>? QuotesCollection;

    private readonly AllMiniLmL6V2Embedder Embedder = new();

    public ServerFeature Feature => new()
    {
        Name = "Quotes",
        Description = "Create memories with this feature.",
        Color = Color.Green,
        IsStandard = false,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureService.GetModuleInfo<QuotesCommand>
    };


    public async Task InitializeAsync()
    {
        _ = await VectorDatabaseService.RequestDatabase("quotes_db");
        QuotesCollection = await VectorDatabaseService.RequestCollection<Quote>();

        foreach (var server in ServerService.Servers) _ = await QuotesCollection.AddPartition(server.Id.ToString());
    }


    public async Task AddForServerAsync(ulong id) => await QuotesCollection!.AddPartition(id.ToString());
    public async Task RemoveForServerAsync(ulong serverID, bool reset) => await QuotesCollection!.RemovePartition(new(serverID.ToString()), reset);

    public async Task<List<Quote>> GetQuotes(ulong serverID) => await QuotesCollection!.GetAllEntities(new(serverID.ToString())).ToListAsync();
    public async Task<OneOf<Quote, NotFound>> GetQuote(Guid quoteID, ulong serverID) => await QuotesCollection!.GetEntity(quoteID, new(serverID.ToString()));
    public async Task<OneOf<Quote, None>> GetRandomQuote(ulong serverID)
    {
        var entities = await QuotesCollection!.GetAllEntities(new(serverID.ToString())).ToListAsync();
        if (entities.Count is 0) return new None();

        var randomIndex = Random.Shared.Next(0, entities.Count);
        var quote = await GetQuote(entities[randomIndex].Id, serverID);

        return quote.IsT0 ? quote.AsT0 : new None();
    }
    public async Task<OneOf<Quote, None>> GetMatchingQuote(string message, ulong serverID)
    {
        if ((await QuotesCollection!.GetAllEntities(new(serverID.ToString())).ToListAsync()).Count is 0) return new None();

        var embedding = Embedder.GenerateEmbedding(message);
        return await QuotesCollection.GetMatchingEntity([.. embedding], new(serverID.ToString()));
    }

    public async Task AddQuote(Quote quote, ulong serverID)
    {
        quote.EmbeddedContent = [.. Embedder.GenerateEmbedding(quote.Context)];
        await QuotesCollection!.AddEntity(quote, new(serverID.ToString()));
    }
    public async Task RemoveQuote(Guid quoteID, ulong serverID) => await QuotesCollection!.RemoveEntity(quoteID, new(serverID.ToString()));
}
