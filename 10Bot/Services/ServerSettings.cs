using Discord.WebSocket;
using Newtonsoft.Json;

namespace TenBot.Services;
public sealed class ServerSettings : IService
{
	private readonly TenBotSettings Configuration;

	public Dictionary<ulong, ServerConfiguration> Configurations { get; } = new();


	public ServerSettings(TenBotSettings configuration, DiscordSocketClient client)
	{
		Configuration = configuration;

		foreach (var server in Directory.GetDirectories(Configuration.RootPath + "/Servers")
										.Select(x =>
										{
											var ServerID = Convert.ToUInt64(x.Split(@"\").Select(x => x.Split("/")).Last().Last());
											var Configuration = GetServerConfiguration<ServerConfiguration>(ServerID, "config.json");
											return (ServerID, Configuration);
										})
										.Where(x => x.Configuration.IsBeta == Configuration.IsBeta))
			Configurations.Add(server.ServerID, server.Configuration);

		client.JoinedGuild += GuildAddedAsync;
	}

	private async Task GuildAddedAsync(SocketGuild server)
	{
		_ = Directory.CreateDirectory(Configuration.RootPath + $"/Servers/{server.Id}");

		var creationCategory = await server.CreateCategoryChannelAsync("➕ Voice Channel Creation");
		var newTalkChannel = await server.CreateVoiceChannelAsync("🆕 New Talk", x => x.CategoryId = creationCategory.Id);
		var newPrivateTalkChannel = await server.CreateVoiceChannelAsync("🔒 New Private Talk", x => x.CategoryId = creationCategory.Id);

		Configurations.Add(server.Id, new(newTalkChannel.Id, newPrivateTalkChannel.Id, , , Configuration.IsBeta));
	}

	public T GetServerConfiguration<T>(ulong serverID, string fileName) where T : class
	{
		if (!File.Exists(Configuration.RootPath + $"/Servers/{serverID}/{fileName}")) File.Create(Configuration.RootPath + $"/Servers/{serverID}/{fileName}").Close();
		return JsonConvert.DeserializeObject<T>(File.ReadAllText(Configuration.RootPath + $"/Servers/{serverID}/{fileName}"))!;
	}
	public Dictionary<ulong, T> GetAllServerConfiguration<T>(string fileName) where T : class
		=> Configurations.Keys.Select(x => new KeyValuePair<ulong, T>(x, GetServerConfiguration<T>(x, fileName))).ToDictionary(key => key.Key, value => value.Value);

	public void SaveServerConfiguration<T>(ulong serverID, string fileName, T content) where T : class
		=> File.WriteAllText(Configuration.RootPath + $"/Servers/{serverID}/{fileName}", JsonConvert.SerializeObject(content));
	public void SaveAllServerConfiguration<T>(string fileName, Dictionary<ulong, T> contents) where T : class
		=> contents.ToList().ForEach(x => SaveServerConfiguration(x.Key, fileName, x.Value));

	public void WatchFile<T>(ulong serverID, string fileName, Action<T> contentUpdated) where T : class
		=> new FileSystemWatcher
		{
			EnableRaisingEvents = true,
			Filter = fileName,
			NotifyFilter = NotifyFilters.LastWrite,
			Path = Configuration.RootPath + $"/Servers/{serverID}"
		}.Changed += (_, _) => contentUpdated(GetServerConfiguration<T>(serverID, fileName));
	//public void WatchAllFiles<T>(string fileName, Action<Dictionary<ulong, T>> contentUpdated) where T : class
	//	=> Configurations.Keys.ToList().ForEach(x => WatchFile(x, fileName, contentUpdated))
}

public sealed record ServerConfiguration(ulong NewTalkChannelID, ulong NewPrivateTalkChannelID, ulong VoiceCategoryID, bool IsRoleSelectionEnabled, bool IsBeta);
