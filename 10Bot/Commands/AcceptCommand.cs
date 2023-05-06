using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class AcceptCommand : InteractionModuleBase
{
	private readonly ServerSettings ServerSettings;


	public AcceptCommand(ServerSettings serverSettings) => ServerSettings = serverSettings;


	[SlashCommand("accept", "Accept the rules to get the 'Member' role!")]
	public async Task AcceptAsync()
	{
		var server = ServerSettings.Configurations[Context.Guild.Id];

		if (Context.User is not IGuildUser user)
		{
			await RespondAsync("Your account wasn't found... Please report that!", ephemeral: true);
			return;
		}

		if (user.RoleIds.Contains(server.MemberRoleID))
		{
			await RespondAsync("Really? You already have this role -_-", ephemeral: true);
			return;
		}

		await user.AddRoleAsync(server.MemberRoleID);
		await RespondAsync("Thank you, have fun!", ephemeral: true);
	}
}
