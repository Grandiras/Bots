using Discord;
using Discord.Interactions;
using TenBot.Helpers;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.SendMessages)]
public sealed class ExecuteCommand : InteractionModuleBase
{
    private readonly CustomCommands CustomCommands;


    public ExecuteCommand(CustomCommands customCommands) => CustomCommands = customCommands;


    [SlashCommand("execute", "Executes a custom command.")]
    public async Task ExecuteAsync([Summary("name", "The name of the command."), Autocomplete(typeof(CommandAutoCompleteHandler))] string name) 
        => await RespondAsync(CustomCommands.GetCommand(name, Context.Guild.Id)!.Content);
}
