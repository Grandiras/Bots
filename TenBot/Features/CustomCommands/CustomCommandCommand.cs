using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.CustomCommands;
[Group("command", "Create custom commands for your server!"), DefaultMemberPermissions(GuildPermission.ManageMessages)]
public sealed class CustomCommandCommand : InteractionModuleBase<ServerInteractionContext>
{
    private readonly CustomCommandsService CustomCommandsService;


    public CustomCommandCommand(CustomCommandsService customCommandsService) => CustomCommandsService = customCommandsService;


    // TODO allow for more complex content
    [SlashCommand("create", "Creates a new custom command.")]
    public async Task CreateAsync([Summary("name", "The name of the command.")] string name,
                                  [Summary("content", "This text will get displayed on execution.")] string content)
    {
        if (CustomCommandsService.CommandExists(name, Context.ServerID))
        {
            await RespondAsync($"A command named '{name}' has already been added!", ephemeral: true);
            return;
        }

        CustomCommandsService.AddCommand(new(name, content), Context.ServerID);
        await RespondAsync($"Command named '{name}' was successfully created.", ephemeral: true);
    }

    [SlashCommand("delete", "Deletes a custom command.")]
    public async Task DeleteAsync([Summary("name", "The name of the command to delete."), Autocomplete(typeof(CustomCommandsAutoCompleteHandler))] string name)
    {
        if (!CustomCommandsService.CommandExists(name, Context.ServerID))
        {
            await RespondAsync($"A command named '{name}' doesn't exist!", ephemeral: true);
            return;
        }

        CustomCommandsService.RemoveCommand(name, Context.ServerID);
        await RespondAsync($"Command named '{name}' was successfully deleted.", ephemeral: true);
    }

    [SlashCommand("modify", "Changes a custom command's content.")]
    public async Task ModifyAsync([Summary("name", "The command's name to be modified."), Autocomplete(typeof(CustomCommandsAutoCompleteHandler))] string name,
                                  [Summary("new_content", "Enter a new content for the selected command!")] string newContent)
    {
        if (!CustomCommandsService.CommandExists(name, Context.ServerID))
        {
            await RespondAsync($"A command named '{name}' doesn't exist!", ephemeral: true);
            return;
        }

        CustomCommandsService.ModifyCommand(name, newContent, Context.ServerID);
        await RespondAsync($"Successfully updated the content of the command '{name}'.", ephemeral: true);
    }

    [SlashCommand("list", "Lists all custom commands.")]
    public async Task ListAsync()
    {
        var embed = new EmbedBuilder()
            .WithTitle("Custom commands")
            .WithColor(CustomCommandsService.Feature.Color);

        foreach (var item in CustomCommandsService.GetCommands(Context.ServerID)) _ = embed.AddField(item.Name, item.Content);

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}