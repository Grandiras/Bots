using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.CustomCommands;
[Group("execute", "Executes a custom command."), DefaultMemberPermissions(GuildPermission.SendMessages)]
public sealed class ExecuteCommand : InteractionModuleBase<ServerInteractionContext>
{
    private readonly CustomCommandsService CustomCommandsService;


    public ExecuteCommand(CustomCommandsService customCommands) => CustomCommandsService = customCommands;


    [SlashCommand("execute", "Executes a custom command.")]
    public async Task ExecuteAsync([Summary("name", "The name of the command."), Autocomplete(typeof(CustomCommandsAutoCompleteHandler))] string name)
    {
        var command = CustomCommandsService.GetCommand(name, Context.Guild.Id);

        if (command.IsT1)
        {
            await RespondAsync("Command not found.");
            return;
        }

        await RespondAsync(command.AsT0.Content);
    }
}
