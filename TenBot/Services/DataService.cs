using OneOf;
using OneOf.Types;
using System.Text.Json;
using TenBot.Configuration;

namespace TenBot.Services;
public sealed class DataService : IService
{
    private readonly BotConfiguration Configuration;

    private string DataRootPath => Path.Combine(Directory.GetCurrentDirectory(), Configuration.DataRootPath);


    public DataService(BotConfiguration configuration)
    {
        Configuration = configuration;

        if (!Directory.Exists(DataRootPath)) _ = Directory.CreateDirectory(Configuration.DataRootPath);
    }


    public OneOf<List<string>, NotFound> GetAllDirectories(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        return Directory.Exists(fullPath) ? Directory.GetDirectories(fullPath).Select(x => Path.GetFileName(x)!.ToString()).ToList() : new global::OneOf.Types.NotFound();
    }
    public OneOf<List<string>, NotFound> GetAllFiles(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        return Directory.Exists(fullPath) ? Directory.GetFiles(fullPath).Select(x => Path.GetFileName(x)).ToList() : new NotFound();
    }

    public bool FileExists(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);
        return File.Exists(fullPath);
    }

    public async Task<OneOf<T, NotFound>> ReadFromFileAsync<T>(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        return File.Exists(fullPath) ? JsonSerializer.Deserialize<T>(await File.ReadAllTextAsync(fullPath))! : new NotFound();
    }
    public async Task SaveToFileAsync<T>(string path, T model, bool createDirectory)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        if (createDirectory && !Directory.Exists(Path.GetDirectoryName(fullPath))) _ = Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
        var contents = JsonSerializer.Serialize(model);
        await File.WriteAllTextAsync(fullPath, contents);
    }

    public async Task<OneOf<List<T>, NotFound>> ReadFromConcurrentFilesAsync<T>(string path, string fileName)
    {
        var results = new List<T>();

        var serverDirectories = GetAllDirectories(path);
        if (serverDirectories.IsT1) return serverDirectories.AsT1;

        foreach (var directory in serverDirectories.AsT0)
        {
            var fullPath = Path.Combine(DataRootPath, path, directory, fileName);
            if (!File.Exists(fullPath)) continue;

            results.Add((await ReadFromFileAsync<T>(fullPath)).AsT0);
        }

        return results;
    }

    public async Task DeleteDirectoryAsync(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        if (Directory.Exists(fullPath)) Directory.Delete(fullPath, true);

        await Task.CompletedTask;
    }
    public async Task DeleteFileAsync(string path)
    {
        var fullPath = Path.Combine(DataRootPath, path);

        if (File.Exists(fullPath)) File.Delete(fullPath);

        await Task.CompletedTask;
    }
}
