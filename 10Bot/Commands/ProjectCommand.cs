using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Helpers;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
[Group("project", "A command to set up and manage projects.")]
public sealed class ProjectCommand : InteractionModuleBase
{
    private readonly ServerService ServerService;
    private readonly ProjectTemplates ProjectTemplates;


    public ProjectCommand(ServerService serverService, ProjectTemplates projectTemplates)
    {
        ServerService = serverService;
        ProjectTemplates = projectTemplates;
    }


    [SlashCommand("create", "Creates a new project.")]
    public async Task CreateAsync([Summary("name", "The project's name.")] string name,
                                  [Summary("template", "The project pattern, which should be used."),
                                   Autocomplete(typeof(ProjectTypeAutoCompleteHandler))] string template)
    {
        var projectTemplate = ProjectTemplates.Templates[template];

        var role = await ServerService.Server.CreateRoleAsync($"{name} - Project", isMentionable: true);
        var category = await ServerService.Server.CreateCategoryChannelAsync(name);

        await RespondAsync($"{template} project '{name}' was successfully created.", ephemeral: true);

        await SetProjectChannelPermissionsAsync(ServerService.Server, role, category);

        foreach (var channel in projectTemplate.Channels) await CreateProjectChannelAsync(ServerService.Server, role, channel, category);
        await (Context.User as IGuildUser)!.AddRoleAsync(role);
    }

    [SlashCommand("delete", "Deletes an existing project.")]
    public async Task DeleteAsync([Summary("name", "The project's name."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string name)
    {
        var role = ServerService.GetRole(x => x.Name.Split(" -")[0] == name);
        var category = ServerService.GetCategoryByRole(role);

        await RespondAsync($"Project '{category.Name}' was successfully deleted.", ephemeral: true);
        
        foreach (var channel in category.Channels) await channel.DeleteAsync();
        await category.DeleteAsync();
        await role.DeleteAsync();
    }

    [SlashCommand("invite", "Invites another person into a project.")]
    public async Task InviteAsync([Summary("project", "The project to invite somebody in."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string project,
                                  [Summary("user", "The user you want to invite.")] IGuildUser user)
    {
        var role = ServerService.GetRole(x => x.Name.Split(" -")[0] == project);

        if (user.RoleIds.Any(x => x == role.Id))
        {
            await RespondAsync("This user has already been invited to this project!", ephemeral: true);
            return;
        }

        await user.AddRoleAsync(role);
        await RespondAsync($"User '{user.Username}' has been added to '{project}'.", ephemeral: true);
    }


    [Group("template", "Used to manage and explain project templates.")]
    public sealed class TemplateCommand : InteractionModuleBase
    {
        private readonly ProjectTemplates ProjectTemplates;


        public TemplateCommand(ProjectTemplates projectTemplates) => ProjectTemplates = projectTemplates;



        [SlashCommand("list", "Displayes all project templates.")]
        public async Task ListAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Purple)
                .WithTitle("Available project templates");

            foreach ((var projectType, var projectTemplate) in ProjectTemplates.Templates)
            {
                _ = embed.AddField(new EmbedFieldBuilder()
                                   .WithName(projectType.ToString())
                                   .WithValue(projectTemplate.Description));
            }

            await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
        }

        [SlashCommand("explain", "Explains a project template.")]
        public async Task ExplainAsync([Summary("template", "The template you want to get explained."),
                                        Autocomplete(typeof(ProjectTypeAutoCompleteHandler))] string template)
        {
            var projectTemplate = ProjectTemplates.Templates[template];

            var embed = new EmbedBuilder()
                .WithColor(Color.Purple)
                .WithTitle(template.ToString());

            _ = embed.AddField(new EmbedFieldBuilder()
                           .WithName("Description")
                           .WithValue(projectTemplate.Description));

            foreach (var channel in projectTemplate.Channels)
            {
                _ = embed.AddField(new EmbedFieldBuilder()
                               .WithName($"Includes channel '{channel.Name}'")
                               .WithValue("Type: " + channel.Kind.ToString()));
            }

            await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
        }

        [SlashCommand("create", "Creates a new template from a discord category.")]
        public async Task CreateAsync([Summary("name", "The name of the template (has to be unique!).")] string name,
                                      [Summary("description", "Describe, what this template is for.")] string description,
                                      [Summary("category", "The category, from which the template will be created.")] SocketCategoryChannel category)
        {
            if (ProjectTemplates.Templates.ContainsKey(name))
            {
                await RespondAsync("A project template with this name already exists!", ephemeral: true);
                return;
            }

            try
            {
                ProjectTemplates.CreateProjectTemplate(name, description, category);
                await RespondAsync($"Project template '{name}' has successfully been created.", ephemeral: true);
            }
            catch (NotSupportedException e)
            {
                await RespondAsync(e.Message, ephemeral: true);
            }
        }
    }


    private static async Task SetProjectChannelPermissionsAsync(SocketGuild server, RestRole role, RestGuildChannel channel)
    {
        await channel.AddPermissionOverwriteAsync(server.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
        await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(viewChannel: PermValue.Allow));
    }

    private static async Task CreateProjectChannelAsync(SocketGuild server, RestRole role, ProjectTemplateChannel channel,
                                                        RestCategoryChannel category)
    {
        if (channel.Kind == ProjectTemplateChannelKind.Text)
        {
            var textChannel = await server.CreateTextChannelAsync(channel.Name, x => x.CategoryId = category.Id);
            await SetProjectChannelPermissionsAsync(server, role, textChannel);
        }
        else if (channel.Kind == ProjectTemplateChannelKind.Voice)
        {
            var voiceChannel = await server.CreateVoiceChannelAsync(channel.Name, x => x.CategoryId = category.Id);
            await SetProjectChannelPermissionsAsync(server, role, voiceChannel);
        }
        else throw new NotSupportedException($"ProjectTemplateChannelKind value '{channel.Kind}' not supported!");
    }
}
