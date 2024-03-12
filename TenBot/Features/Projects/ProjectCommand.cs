using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.ServerAbstractions;

namespace TenBot.Features.Projects;
[Group("project", "Create temporary categories for projects, that won't last forever!"), DefaultMemberPermissions(GuildPermission.ManageChannels)]
public sealed class ProjectCommand(ProjectService ProjectService) : InteractionModuleBase<ServerInteractionContext>
{
    [SlashCommand("create", "Creates a new project.")]
    public async Task CreateAsync([Summary("name", "The project's name.")] string name,
                                  [Summary("template", "The project pattern, which should be used."),
                                   Autocomplete(typeof(ProjectTemplateAutoCompleteHandler))] string template,
                                  [Summary("is_public", "Determines, whether this project will be discoverable through role selection.")] bool isPublic = true)
    {
        var fixedName = name.Replace(" - Project", "").Replace(" - Private", "");
        var projectTemplate = ProjectService.GetTemplate(template, Context.ServerID);

        if (projectTemplate.IsT1)
        {
            await RespondAsync($"'{template}' is not a valid project template!", ephemeral: true);
            return;
        }

        if (Context.Guild.Roles.Any(x => x.Name.Split(" - Project").First() == name))
        {
            await RespondAsync($"A project with the name '{name}' does already exist!", ephemeral: true);
            return;
        }

        var role = await Context.Guild.CreateRoleAsync($"{fixedName} - Project{(isPublic ? "" : " - Private")}");
        var category = await Context.Guild.CreateCategoryChannelAsync(fixedName);

        await RespondAsync($"{template} project '{fixedName}' was successfully created.", ephemeral: true);

        await category.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
        await category.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));

        foreach (var channel in projectTemplate.AsT0.Channels)
        {
            RestGuildChannel restChannel = channel.Kind switch
            {
                ProjectDataChannelKind.Text => await Context.Guild.CreateTextChannelAsync(channel.Name, x => x.CategoryId = category.Id),
                ProjectDataChannelKind.Voice => await Context.Guild.CreateVoiceChannelAsync(channel.Name, x => x.CategoryId = category.Id),
                ProjectDataChannelKind.Stage => await Context.Guild.CreateStageChannelAsync(channel.Name, x => x.CategoryId = category.Id),
                ProjectDataChannelKind.Forum => await Context.Guild.CreateForumChannelAsync(channel.Name, x => x.CategoryId = category.Id),
                _ => throw new NotSupportedException($"ProjectTemplateChannelKind value '{channel.Kind}' not supported!"),
            };

            await restChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
            await restChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));
        }

        await Context.User.AddRoleAsync(role);
    }

    [SlashCommand("delete", "Deletes an existing project.")]
    public async Task DeleteAsync([Summary("name", "The project's name."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string name)
    {
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.Split(" - ")[0] == name);
        var category = Context.Guild.CategoryChannels.FirstOrDefault(x => x.Name == name);

        if (role is null || category is null)
        {
            await RespondAsync($"Project '{name}' does not exist!", ephemeral: true);
            return;
        }

        await RespondAsync($"Project '{category.Name}' was successfully deleted.", ephemeral: true);

        foreach (var channel in category.Channels) await channel.DeleteAsync();
        await category.DeleteAsync();
        await role.DeleteAsync();
    }

    [SlashCommand("list", "Lists all existing projects.")]
    public async Task ListAsync()
    {
        var projects = Context.Guild.Roles.Where(x => x.Name.Contains(" - Project")).Select(x => x.Name).ToList();

        var embed = new EmbedBuilder()
            .WithTitle("Available projects")
            .WithColor(ProjectService.Feature.Color);

        foreach (var project in projects) _ = embed.AddField(project.Split(" - ")[0], project.Split(" - ").Last() == "Private" ? "private" : "public");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [SlashCommand("invite", "Invites another person into a project.")]
    public async Task InviteAsync([Summary("project", "The project to invite somebody in."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string project,
                                  [Summary("user", "The user you want to invite.")] IGuildUser user)
    {
        var role = Context.Guild.Roles.FirstOrDefault(x => x.Name.Split(" - ")[0] == project);

        if (role is null)
        {
            await RespondAsync($"Project '{project}' does not exist!", ephemeral: true);
            return;
        }

        if (user.RoleIds.Any(x => x == role.Id))
        {
            await RespondAsync($"{user.Mention} has already been invited to this project!", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(role);
        await RespondAsync($"{user.Mention} has been invited to '{project}'.", ephemeral: true);
    }

    [SlashCommand("discover", "Discover great things on this server!"), DefaultMemberPermissions(GuildPermission.SendMessages)]
    public async Task DiscoverAsync()
    {
        var projects = Context.Guild.Roles.Where(x => x.Name.EndsWith(" - Project")).Select(x => x.Name.Split(" - ").First()).ToList();

        if (projects.Count is 0)
        {
            await RespondAsync("I'm sorry, but there aren't any projects you could join...", ephemeral: true);
            return;
        }

        var selector = new SelectMenuBuilder()
            .WithPlaceholder("Select the projects you want to join")
            .WithCustomId("discover_projects")
            .WithMinValues(0)
            .WithMaxValues(projects.Count);

        foreach (var project in projects) _ = selector.AddOption(project, project);

        var builder = new ComponentBuilder().WithSelectMenu(selector);

        await RespondAsync(components: builder.Build(), ephemeral: true);
    }

    [ComponentInteraction("discover_projects", true)]
    public async Task DiscoverSubmittedAsync(string[] selectedProjects)
    {
        foreach (var project in selectedProjects)
        {
            var role = Context.Guild.Roles.First(x => x.Name.Contains(" - Project"));
            if (!Context.User.Roles.Any(x => x.Id == role.Id)) await Context.User.AddRoleAsync(role);
        }

        await RespondAsync("You were successfully added to these projects!", ephemeral: true);
    }

    [Group("template", "Used to manage and explain project templates.")]
    public class TemplateCommand(ProjectService ProjectService) : InteractionModuleBase<ServerInteractionContext> // TODO allow for project-dependent moderation
    {
        [SlashCommand("list", "Displays all project templates.")]
        public async Task ListAsync()
        {
            var embed = new EmbedBuilder()
                .WithTitle("Available project templates")
                .WithColor(ProjectService.Feature.Color);

            foreach (var template in ProjectService.GetTemplates(Context.ServerID)) _ = embed.AddField(template.Name, template.Description);

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("explain", "Explains a project template.")]
        public async Task ExplainAsync([Summary("template", "The template you want to get explained."),
                                        Autocomplete(typeof(ProjectTemplateAutoCompleteHandler))] string template)
        {
            var projectTemplate = ProjectService.GetTemplate(template, Context.ServerID);

            if (projectTemplate.IsT1)
            {
                await RespondAsync($"'{template}' is not a valid project template!", ephemeral: true);
                return;
            }

            var embed = new EmbedBuilder()
                .WithTitle(projectTemplate.AsT0.Name)
                .WithColor(ProjectService.Feature.Color);

            _ = embed.AddField("Description", projectTemplate.AsT0.Description);

            foreach (var channel in projectTemplate.AsT0.Channels) _ = embed.AddField($"Includes channel '{channel.Name}'", $"Type: {channel.Kind}");

            await RespondAsync(embed: embed.Build(), ephemeral: true);
        }

        [SlashCommand("create", "Creates a new project template.")]
        public async Task CreateAsync([Summary("name", "The name of the template (has to be unique!).")] string name,
                                      [Summary("description", "Describe, what this template is for.")] string description,
                                      [Summary("category", "The category, from which the template will be created.")] SocketCategoryChannel category)
        {
            if (ProjectService.GetTemplates(Context.ServerID).Any(x => x.Name == name))
            {
                await RespondAsync($"A project template with the name '{name}' already exists!", ephemeral: true);
                return;
            }

            try
            {
                ProjectService.AddTemplate(new(name, description, []), category);
                await RespondAsync($"Project template '{name}' has successfully been created.", ephemeral: true);
            }
            catch (NotSupportedException e)
            {
                await RespondAsync(e.Message, ephemeral: true);
            }
        }

        [SlashCommand("delete", "Deletes an existing project template.")]
        public async Task DeleteAsync([Summary("name", "The name of the template you want to delete."),
                                       Autocomplete(typeof(ServerProjectTemplateAutoCompleteHandler))] string name)
        {
            if (!ProjectService.GetTemplates(Context.ServerID, false).Any(x => x.Name == name))
            {
                await RespondAsync($"Template '{name}' is not a valid project template!", ephemeral: true);
                return;
            }

            ProjectService.RemoveTemplate(name, Context.ServerID);
            await RespondAsync($"Project template '{name}' has successfully been deleted.", ephemeral: true);
        }
    }
}
