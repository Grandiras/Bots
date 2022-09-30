using Discord.Interactions;
using TenBot.Helpers;
using TenBot.Services;

namespace TenBot.Commands;
public sealed class ExecuteCommand : InteractionModuleBase
{
    private readonly CustomCommands CustomCommands;


    public ExecuteCommand(CustomCommands customCommands) => CustomCommands = customCommands;


    [SlashCommand("execute", "Executes a custom command.")]
    public async Task ExecuteAsync([Summary("name", "The name of the command."), Autocomplete(typeof(CommandAutoCompleteHandler))] string name)
    {
        if (!CustomCommands.CommandExists(name))
        {
            await RespondAsync($"A command named '{name}' was not found!", ephemeral: true);
            return;
        }

        await RespondAsync(CustomCommands.GetCommand(name)!.Content);
    }
}
