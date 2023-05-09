using TenBot.Models;

namespace TenBot.Services;
public sealed class CustomCommands : IService, IDisposable
{
	private const string FILE_NAME = "custom_commands.json";

	private readonly ServerSettings ServerSettings;

	private readonly Dictionary<ulong, List<CustomCommand>> Commands = new();

    private readonly SettingsService Settings;


	public CustomCommands(ServerSettings serverSettings)
	{
		ServerSettings = serverSettings;

		Commands = ServerSettings.GetAllServerConfiguration<List<CustomCommand>>(FILE_NAME);
	}


	public bool CommandExists(string name, ulong serverID) => Commands[serverID].Any(c => c.Name == name);

	public CustomCommand? GetCommand(string name, ulong serverID) => Commands[serverID].FirstOrDefault(c => c.Name == name);
	public List<CustomCommand> GetCommands(ulong serverID) => Commands[serverID];

	public void AddCommand(CustomCommand command, ulong serverID)
	{
		Commands[serverID].Add(command);
		SaveCommands(serverID);
	}
	public void RemoveCommand(string name, ulong serverID)
	{
		_ = Commands[serverID].Remove(Commands[serverID].First(c => c.Name == name));
		SaveCommands(serverID);
	}

	public void ModifyCommand(string name, string newContent, ulong serverID)
	{
		Commands[serverID].First(c => c.Name == name).Content = newContent;
		SaveCommands(serverID);
	}

	private void SaveCommands(ulong serverID) => ServerSettings.SaveServerConfiguration(serverID, FILE_NAME, Commands[serverID]);

	public void Dispose() => ServerSettings.SaveAllServerConfiguration(FILE_NAME, Commands);
}
