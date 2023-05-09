using Discord;

namespace TenBot.Services;
public sealed class WelcomeMessages : IService, IDisposable
{
	private const string FILE_NAME = "welcome_messages.json";

	private readonly ServerSettings ServerSettings;

	private readonly Random Randomizer = new();
	private readonly Dictionary<ulong, List<string>> Messages = new();


	public WelcomeMessages(ServerSettings serverSettings)
	{
		ServerSettings = serverSettings;

		Messages = ServerSettings.GetAllServerConfiguration<List<string>>(FILE_NAME);
	}


	public string GetWelcomeMessage(IGuildUser user)
	{
		var messages = GetWelcomeMessages(user.Guild.Id).ToList();
		return messages[Randomizer.Next(messages.Count - 1)].Replace("[]", user.Mention);
	}
	public IEnumerable<string> GetWelcomeMessages(ulong serverID) => Messages[serverID];

	public void AddWelcomeMessage(string message, ulong serverID)
	{
		var messages = GetWelcomeMessages(serverID).ToList();
		messages.Add(message);

		ServerSettings.SaveServerConfiguration(serverID, FILE_NAME, messages);
	}

	public void Dispose() => ServerSettings.SaveAllServerConfiguration(FILE_NAME, Messages);
}
