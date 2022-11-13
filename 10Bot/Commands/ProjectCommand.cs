using Discord.Interactions;
using TenBot.Enums;
using TenBot.Helpers;

namespace TenBot.Commands;
[DefaultMemberPermissions(Discord.GuildPermission.ManageChannels)]
[Group("project", "A command to set up and manage projects.")]
public class ProjectCommand : InteractionModuleBase
{
    [SlashCommand("create", "Creates a new project.")]
    public async Task CreateAsync([Summary("name", "The project's name.")] string name,
                                  [Summary("type", "The project pattern, which should be used."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] ProjectType type)
    {

    }
}
