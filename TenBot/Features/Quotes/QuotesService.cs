using AllMiniLmL6V2Sharp;
using Discord;
using Milvus.Client;
using OneOf;
using OneOf.Types;
using System.Reactive.Linq;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Features.Quotes;
public sealed class QuotesService : IFeature, IMustInitialize
{
    private readonly FeatureService FeatureService;
    private readonly ServerService ServerService;
    private readonly ILogger<QuotesService> Logger;

    private readonly MilvusClient QuotesDB;
    private MilvusCollection? QuotesCollection;

    private readonly AllMiniLmL6V2Embedder Embedder;

    public ServerFeature Feature => new()
    {
        Name = "Quotes",
        Description = "Create memories with this feature.",
        Color = Color.Green,
        IsStandard = false,
        FeatureReference = this,
        CommandHandlerModuleHandler = FeatureService.GetModuleInfo<QuotesCommand>
    };


    public QuotesService(FeatureService featureService, ServerService serverService, ILogger<QuotesService> logger, MilvusClient quotesDB)
    {
        FeatureService = featureService;
        ServerService = serverService;
        Logger = logger;

        QuotesDB = quotesDB;

        Embedder = new AllMiniLmL6V2Embedder();
    }

    public async Task InitializeAsync()
    {
        if (!(await QuotesDB.ListDatabasesAsync()).Any(db => db == "quotes_db")) await QuotesDB.CreateDatabaseAsync("quotes_db");

        if (await QuotesDB.HasCollectionAsync("quotes")) QuotesCollection = QuotesDB.GetCollection("quotes");
        else
        {
            var schema = new CollectionSchema
            {
                Fields =
                {
                    FieldSchema.CreateVarchar("quote_id", maxLength: 100, isPrimaryKey: true),
                    FieldSchema.CreateFloatVector("quote_embedded_content", dimension: 384),
                    FieldSchema.CreateVarchar("quote_content", maxLength: 2000),
                    FieldSchema.CreateVarchar("quote_author", maxLength: 200),
                    FieldSchema.CreateVarchar("quote_context", maxLength: 500)
                }
            };

            QuotesCollection = await QuotesDB.CreateCollectionAsync("quotes", schema);
            await QuotesCollection.CreateIndexAsync("quote_embedded_content");

            await QuotesCollection.LoadAsync();
        }

        foreach (var server in ServerService.Servers)
            if (!await QuotesCollection.HasPartitionAsync($"quotes_{server.Id}"))
                await QuotesCollection.CreatePartitionAsync($"quotes_{server.Id}");

        foreach (var server in ServerService.Servers) await QuotesCollection.LoadPartitionAsync($"quotes_{server.Id}");

        _ = await QuotesCollection.FlushAsync();
    }


    public async Task AddForServerAsync(ulong id)
    {
        if (!await QuotesCollection!.HasPartitionAsync($"quotes_{id}")) await QuotesCollection.CreatePartitionAsync($"quotes_{id}");
        await QuotesCollection.LoadPartitionAsync($"quotes_{id}");

        _ = await QuotesCollection.FlushAsync();
    }
    public async Task RemoveForServerAsync(ulong serverID, bool reset)
    {
        await QuotesCollection!.ReleasePartitionAsync($"quotes_{serverID}");
        if (reset) await QuotesCollection.DropPartitionAsync($"quotes_{serverID}");

        _ = await QuotesCollection.FlushAsync();
    }

    public async Task<List<QuotesData>> GetQuotes(ulong serverID)
    {
        var fields = await QuotesCollection!.QueryAsync("quote_id like \"%\"", new QueryParameters { PartitionNames = { $"quotes_{serverID}" }, OutputFields = { "quote_id", "quote_content", "quote_author", "quote_context" } });

        var quoteIDs = ((FieldData<string>)fields.First(x => x.FieldName is "quote_id")).Data;
        var quoteContents = ((FieldData<string>)fields.First(x => x.FieldName is "quote_content")).Data;
        var quoteAuthors = ((FieldData<string>)fields.First(x => x.FieldName is "quote_author")).Data;
        var quoteContexts = ((FieldData<string>)fields.First(x => x.FieldName is "quote_context")).Data;

        return [.. quoteIDs.Select((quoteID, index) => new QuotesData(Guid.Parse(quoteID), quoteContents[index], quoteAuthors[index], !quoteContexts[index].IsWhiteSpace() ? quoteContexts[index] : null))];
    }
    public async Task<OneOf<QuotesData, NotFound>> GetQuote(Guid quoteID, ulong serverID)
    {
        var queryResult = await QuotesCollection!.QueryAsync($"quote_id like \"{quoteID}\"", new QueryParameters { OutputFields = { "quote_id", "quote_content", "quote_author", "quote_context" }, PartitionNames = { $"quotes_{serverID}" } });
        if (queryResult.Count is 0) return new NotFound();

        var quoteIDField = (FieldData<string>)queryResult.First(x => x.FieldName is "quote_id");
        var quoteContentField = (FieldData<string>)queryResult.First(x => x.FieldName is "quote_content");
        var quoteAuthorField = (FieldData<string>)queryResult.First(x => x.FieldName is "quote_author");
        var quoteContextField = (FieldData<string>)queryResult.First(x => x.FieldName is "quote_context");

        return new QuotesData(Guid.Parse(quoteIDField.Data[0]), quoteContentField.Data[0], quoteAuthorField.Data[0], !quoteContextField.Data[0].IsWhiteSpace() ? quoteContextField.Data[0] : null);
    }
    public async Task<OneOf<QuotesData, None>> GetRandomQuote(ulong serverID)
    {
        var quoteIDs = (FieldData<string>)(await QuotesCollection!.QueryAsync("quote_id like \"%\"", new QueryParameters { PartitionNames = { $"quotes_{serverID}" } }))[0];
        if (quoteIDs.RowCount is 0) return new None();

        var randomIndex = Random.Shared.Next(0, (int)quoteIDs.RowCount);
        var quote = await GetQuote(Guid.Parse(quoteIDs.Data[randomIndex]), serverID);

        return quote.IsT0 ? quote.AsT0 : new None();
    }
    public async Task<OneOf<QuotesData, None>> GetMatchingQuote(string message, ulong serverID)
    {
        if ((await QuotesCollection!.QueryAsync("quote_id like \"%\"", new QueryParameters { PartitionNames = { $"quotes_{serverID}" } }))[0].RowCount is 0) return new None();

        var embedding = Embedder.GenerateEmbedding(message);
        var results = await QuotesCollection!.SearchAsync<float>("quote_embedded_content", [new([.. embedding])], SimilarityMetricType.Cosine, 1, new SearchParameters { PartitionNames = { $"quotes_{serverID}" }, ConsistencyLevel = ConsistencyLevel.Strong, OutputFields = { "quote_id", "quote_content", "quote_author", "quote_context" } });

        if (results.Scores[0] < 0.3) return new None();

        var quoteIDField = (FieldData<string>)results.FieldsData.First(x => x.FieldName is "quote_id");
        var quoteContentField = (FieldData<string>)results.FieldsData.First(x => x.FieldName is "quote_content");
        var quoteAuthorField = (FieldData<string>)results.FieldsData.First(x => x.FieldName is "quote_author");
        var quoteContextField = (FieldData<string>)results.FieldsData.First(x => x.FieldName is "quote_context");

        return new QuotesData(Guid.Parse(quoteIDField.Data[0]), quoteContentField.Data[0], quoteAuthorField.Data[0], !quoteContextField.Data[0].IsWhiteSpace() ? quoteContextField.Data[0] : null);
    }

    public async Task AddQuote(QuotesData quote, ulong serverID)
    {
        _ = await QuotesCollection!.InsertAsync([
            FieldData.Create("quote_id", [quote.Id.ToString()]),
            FieldData.CreateFloatVector("quote_embedded_content", [new([.. Embedder.GenerateEmbedding(quote.Quote)])]),
            FieldData.Create("quote_content", [quote.Quote]),
            FieldData.Create("quote_author", [quote.Author]),
            FieldData.Create("quote_context", [quote.Context ?? ""])
        ], $"quotes_{serverID}");

        _ = await QuotesCollection.FlushAsync();
    }
    public async Task RemoveQuote(Guid quoteID, ulong serverID)
    {
        _ = await QuotesCollection!.DeleteAsync($"quote_id like \"{quoteID}\"", $"quotes_{serverID}");
        _ = await QuotesCollection.FlushAsync();
    }
}
