using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.SendMessages)]
[Group("discover", "Discover great things on this server!")]
public sealed class DiscoverCommand : InteractionModuleBase
{
    private readonly ServerService ServerService;

    public DiscoverCommand(ServerService serverService) => ServerService = serverService;



    [SlashCommand("projects", "Discover public projects with this command!")]
    public async Task ProjectsAsync()
    {
        var projects = ServerService.GetRoles(x => x.Name.EndsWith(" - Project - Public"), Context.Guild.Id);

        if (!projects.Any())
        {
            await RespondAsync("I'm sorry, but there aren't any projects you could join...", ephemeral: true);
            return;
        }

        var selector = new SelectMenuBuilder()
            .WithPlaceholder("Select the projects you want to join")
            .WithCustomId("discover_projects")
            .WithMinValues(0)
            .WithMaxValues(projects.Count());

        foreach (var project in projects)
            _ = selector.AddOption(project.Name.Split(" -").First(), project.Name.Split(" -").First().Replace(" ", "-"));

        var builder = new ComponentBuilder()
            .WithSelectMenu(selector);

        await RespondAsync(components: builder.Build(), ephemeral: true);
    }

    [ComponentInteraction("discover_projects", true)]
    public async Task DiscoverSubmittedAsync(string[] selectedProjects)
    {
        foreach (var project in selectedProjects)
        {
            var role = ServerService.GetRole(x => x.Name.StartsWith(project.Replace("-", " ")), Context.Guild.Id);

            if (!((IGuildUser)Context.User).RoleIds.Contains(role.Id)) await ((IGuildUser)Context.User).AddRoleAsync(role);
        }

        await RespondAsync("You were successfully added to these projects!", ephemeral: true);
    }
}
