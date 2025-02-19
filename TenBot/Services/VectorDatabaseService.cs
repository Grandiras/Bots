using Milvus.Client;
using OneOf;
using OneOf.Types;
using System.Reflection;
using TenBot.Helpers;

namespace TenBot.Services;

public sealed class VectorDatabaseService(ILogger<VectorDatabaseService> Logger, MilvusClient DatabaseClient) : IService
{
    public async Task<VectorDatabaseInterface> RequestDatabase(string name)
    {
        if (!(await DatabaseClient.ListDatabasesAsync()).Any(db => db == name)) await DatabaseClient.CreateDatabaseAsync(name);

        return new VectorDatabaseInterface(name);
    }

    public async Task<VectorDatabaseCollection<T>> RequestCollection<T>() where T : class, new()
    {
        var collectionName = typeof(T).Name;

        if (await DatabaseClient.HasCollectionAsync(collectionName)) return new(collectionName, DatabaseClient.GetCollection(collectionName));

        var fields = typeof(T).GetProperties();
        var schema = new CollectionSchema();

        foreach (var field in fields)
        {
            if (field.PropertyType == typeof(Guid)) schema.Fields.Add(FieldSchema.CreateVarchar(field.Name, maxLength: 100, isPrimaryKey: true));
            else if (field.PropertyType == typeof(string)) schema.Fields.Add(FieldSchema.CreateVarchar(field.Name, maxLength: 2048));
            else if (field.PropertyType == typeof(float[])) schema.Fields.Add(FieldSchema.CreateFloatVector(field.Name, dimension: 384));
        }

        var collection = await DatabaseClient.CreateCollectionAsync(collectionName, schema);
        await collection.CreateIndexAsync(fields.First(x => x.PropertyType == typeof(float[])).Name);

        await collection.LoadAsync();

        return new(collectionName, collection);
    }
}

public sealed record VectorDatabaseInterface(string DatabaseName);
public sealed record VectorDatabasePartition(string PartitionName);

public sealed class VectorDatabaseCollection<T> where T : class, new()
{
    private readonly MilvusCollection MilvusCollection;
    private readonly List<(PropertyInfo Property, Type Type, string Name, bool IsPrimaryKey, bool IsIndex)> Fields = [];

    public string CollectionName { get; }

    public List<VectorDatabasePartition> Partitions { get; } = [];


    public VectorDatabaseCollection(string collectionName, MilvusCollection milvusCollection)
    {
        MilvusCollection = milvusCollection;
        CollectionName = collectionName;

        Fields.AddRange(typeof(T).GetProperties().Select(x => (x, x.PropertyType, x.Name, x.PropertyType == typeof(Guid), x.PropertyType == typeof(float[]))));
    }


    public async Task<VectorDatabasePartition> AddPartition(string partitionName)
    {
        if (Partitions.FirstOrDefault(x => x.PartitionName == partitionName) is not null and VectorDatabasePartition existingPartition) return existingPartition;

        if (!await MilvusCollection.HasPartitionAsync(partitionName)) await MilvusCollection.CreatePartitionAsync(partitionName);
        await MilvusCollection.LoadPartitionAsync(partitionName);
        _ = await MilvusCollection.FlushAsync();

        var partition = new VectorDatabasePartition(partitionName);
        Partitions.Add(partition);
        return partition;
    }
    public async Task RemovePartition(VectorDatabasePartition partition, bool delete = false)
    {
        if (!Partitions.Contains(partition)) return;

        await MilvusCollection.ReleasePartitionAsync(partition.PartitionName);
        if (delete) await MilvusCollection.DropPartitionAsync(partition.PartitionName);
    }

    public async IAsyncEnumerable<T> GetAllEntities(VectorDatabasePartition? partition = null)
    {
        var queryParameters = new QueryParameters();
        if (partition is not null) queryParameters.PartitionNames.Add(partition.PartitionName);
        foreach (var field in Fields) queryParameters.OutputFields.Add(field.Name);

        var fields = await MilvusCollection.QueryAsync($"{Fields.First(x => x.IsPrimaryKey).Name} like \"%\"", queryParameters);

        for (var i = 0; i < fields[0].RowCount; i++)
        {
            var entity = CreateEntity(fields, i);
            if (entity.IsT0) yield return entity.AsT0;
        }

        yield break;
    }
    public async Task<OneOf<T, NotFound>> GetEntity(Guid id, VectorDatabasePartition? partition = null)
    {
        var queryParameters = new QueryParameters();
        if (partition is not null) queryParameters.PartitionNames.Add(partition.PartitionName);
        foreach (var field in Fields) queryParameters.OutputFields.Add(field.Name);

        var fields = await MilvusCollection.QueryAsync($"{Fields.First(x => x.IsPrimaryKey).Name} like \"{id}\"", queryParameters);
        return fields[0].RowCount is not 0 ? CreateEntity(fields).Match<OneOf<T, NotFound>>(x => x, none => new NotFound()) : new NotFound();
    }
    public async IAsyncEnumerable<T> GetMatchingEntities(float[] searchEmbedding, uint count, VectorDatabasePartition? partition = null, float threshold = 0.3f)
    {
        var searchParameters = new SearchParameters
        {
            ConsistencyLevel = ConsistencyLevel.Strong
        };
        if (partition is not null) searchParameters.PartitionNames.Add(partition.PartitionName);
        foreach (var field in Fields) searchParameters.OutputFields.Add(field.Name);

        var results = await MilvusCollection.SearchAsync<float>(Fields.First(x => x.IsIndex).Name, [new([.. searchEmbedding])], SimilarityMetricType.Cosine, (int)count, searchParameters);
        var fields = results.FieldsData;

        for (var i = 0; i < results.Scores.Count; i++)
        {
            if (results.Scores[i] < threshold) continue;

            var entity = CreateEntity(fields, i);
            if (entity.IsT0) yield return entity.AsT0;
        }

        yield break;
    }
    public async Task<OneOf<T, None>> GetMatchingEntity(float[] searchEmbedding, VectorDatabasePartition? partition = null, float threshold = 0.3f)
    {
        var searchParameters = new SearchParameters
        {
            ConsistencyLevel = ConsistencyLevel.Strong
        };
        if (partition is not null) searchParameters.PartitionNames.Add(partition.PartitionName);
        foreach (var field in Fields) searchParameters.OutputFields.Add(field.Name);

        var results = await MilvusCollection.SearchAsync<float>(Fields.First(x => x.IsIndex).Name, [new(searchEmbedding)], SimilarityMetricType.Cosine, 1, searchParameters);
        if (results.Scores[0] < threshold) return new None();

        return CreateEntity(results.FieldsData);
    }

    public async Task AddEntity(T entity, VectorDatabasePartition? partition = null)
    {
        var fieldData = new List<FieldData>();
        foreach (var field in Fields)
        {
            if (field.Type == typeof(Guid)) fieldData.Add(FieldData.Create(field.Name, [((Guid)field.Property.GetValue(entity)!).ToString()]));
            else if (field.Type == typeof(string)) fieldData.Add(FieldData.Create(field.Name, [(string)field.Property.GetValue(entity)!]));
            else if (field.Type == typeof(float[])) fieldData.Add(FieldData.CreateFloatVector(field.Name, [(float[])field.Property.GetValue(entity)!]));
        }

        _ = await MilvusCollection.InsertAsync(fieldData, partition?.PartitionName);
        _ = await MilvusCollection.FlushAsync();
    }
    public async Task RemoveEntity(Guid id, VectorDatabasePartition? partition = null)
    {
        _ = await MilvusCollection.DeleteAsync($"{Fields.First(x => x.IsPrimaryKey).Name} like \"{id}\"", partition?.PartitionName);
        _ = await MilvusCollection.FlushAsync();
    }

    private OneOf<T, None> CreateEntity(IReadOnlyList<FieldData> fields, int index = 0)
    {
        var entity = new T();
        foreach (var field in Fields)
        {
            if (field.Type == typeof(float[]))
            {
                field.Property.SetValue(entity, ((FloatVectorFieldData)fields.First(x => x.FieldName == field.Name)).Data[index].ToArray());
                continue;
            }

            dynamic fieldData = fields.First(x => x.FieldName == field.Name).CastToGeneric(typeof(FieldData<>).MakeGenericType(field.Type == typeof(Guid) ? typeof(string) : field.Type))!;
            field.Property.SetValue(entity, field.Type == typeof(Guid) ? Guid.Parse(fieldData.Data[index]) : fieldData.Data[index]);
        }
        return entity;
    }
}