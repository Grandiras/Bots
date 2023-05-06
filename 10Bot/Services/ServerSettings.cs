using Newtonsoft.Json;
using TenBot.Models;

namespace TenBot.Services;
public sealed class ServerSettings : IService
{
	private readonly TenBotSettings Configuration;

	public Dictionary<ulong, ServerConfiguration> Configurations { get; } = new();


	public ServerSettings(TenBotSettings configuration)
	{
		Configuration = configuration;

		foreach (var server in Directory.GetDirectories(Configuration.RootPath + @"\Servers")
										.Select(x =>
										{
											var ServerID = Convert.ToUInt64(x.Split(@"\").Last());
											var Configuration = GetServerConfiguration<ServerConfiguration>(ServerID, "config.json");
											return (ServerID, Configuration);
										})
										.Where(x => x.Configuration.IsBeta == Configuration.IsBeta))
			Configurations.Add(server.ServerID, server.Configuration);
	}


	public T GetServerConfiguration<T>(ulong serverID, string fileName) where T : class
		=> JsonConvert.DeserializeObject<T>(File.ReadAllText(Configuration.RootPath + $@"\Servers\{serverID}\{fileName}"))!;
	public void SaveServerConfiguration<T>(ulong serverID, string fileName, T content) where T : class
		=> File.WriteAllText(Configuration.RootPath + $@"\Servers\{serverID}\{fileName}", JsonConvert.SerializeObject(content));

	public Dictionary<ulong, T> GetAllServerConfiguration<T>(string fileName) where T : class
		=> Configurations.Keys.Select(x => new KeyValuePair<ulong, T>(x, GetServerConfiguration<T>(x, fileName))).ToDictionary(key => key.Key, value => value.Value);
	public void SaveAllServerConfiguration<T>(string fileName, Dictionary<ulong, T> contents) where T : class
		=> contents.ToList().ForEach(x => SaveServerConfiguration(x.Key, fileName, x.Value));
}

public sealed record ServerConfiguration(ulong NewTalkChannelID, ulong NewPrivateTalkChannelID, ulong VoiceCategoryID, ulong MemberRoleID, bool IsRoleSelectionEnabled, bool IsBeta);
