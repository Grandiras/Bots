using AllMiniLmL6V2Sharp;

namespace TenBot.Services;

public sealed class EmbeddingService : IService
{
    private readonly AllMiniLmL6V2Embedder Embedder = new();


    public float[] GenerateEmbedding(string message) => [.. Embedder.GenerateEmbedding(message)];
}
