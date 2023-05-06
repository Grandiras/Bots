using Discord;
using Discord.Interactions;
using TenBot.Helpers;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.ManageMessages)]
[Group("command", "A command to manage custom commands.")]
public sealed class CommandCommand : InteractionModuleBase
{
	private readonly CustomCommands CustomCommands;


	public CommandCommand(CustomCommands customCommands) => CustomCommands = customCommands;


	[SlashCommand("create", "Create a new custom command!")]
	public async Task CreateAsync([Summary("name", "The name of the command.")] string name,
								  [Summary("content", "This text will get displayed on execution.")] string content)
	{
		if (CustomCommands.CommandExists(name, Context.Guild.Id))
		{
			await RespondAsync($"A command named '{name}' has already been added!", ephemeral: true);
			return;
		}

		CustomCommands.AddCommand(new(name, content), Context.Guild.Id);
		await RespondAsync($"Command named '{name}' was successfully created!", ephemeral: true);
	}

	[SlashCommand("delete", "Deletes a custom command.")]
	public async Task DeleteAsync([Summary("name", "The name of the command to delete."),
								   Autocomplete(typeof(CommandAutoCompleteHandler))] string name)
	{
		CustomCommands.RemoveCommand(name, Context.Guild.Id);
		await RespondAsync($"Command named '{name}' was successfully deleted!", ephemeral: true);
	}

	[SlashCommand("modify", "Change a custom command's content!")]
	public async Task ModifyAsync([Summary("name", "The command's name to be modified."),
								   Autocomplete(typeof(CommandAutoCompleteHandler))] string name,
								  [Summary("new_content", "Enter a new content for the selected command!")] string newContent)
	{
		CustomCommands.ModifyCommand(name, newContent, Context.Guild.Id);
		await RespondAsync($"Successfully updated the content of the command '{name}'", ephemeral: true);
	}

	[SlashCommand("list", "Lists all custom commands.")]
	public async Task ListAsync()
	{
		var embed = new EmbedBuilder()
			.WithTitle("Custom commands")
			.WithColor(Color.Blue);

		foreach (var item in CustomCommands.GetCommands(Context.Guild.Id)) _ = embed.AddField(item.Name, item.Content);

		await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
	}
}
