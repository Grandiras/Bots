namespace TenBot.Services;
public sealed class FileSystemManager : IService
{
    public const string SERVER_DIR = "Servers/";

    private readonly SettingsService Settings;


    public FileSystemManager(SettingsService settings) => Settings = settings;


    public void CreateServerDirectoryIfNotExisting(ulong serverID) 
        { if (!Directory.Exists(Settings.RootDirectory + SERVER_DIR + serverID)) _ = Directory.CreateDirectory(Settings.RootDirectory + SERVER_DIR + serverID); }
	public void WriteToServerFile(ulong serverID, string fileName, string content) => File.WriteAllText(Settings.RootDirectory + SERVER_DIR + serverID + $"/{fileName}.json", content);
}
