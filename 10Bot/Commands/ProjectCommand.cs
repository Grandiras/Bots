using Discord;
using Discord.Interactions;
using Discord.Rest;
using Discord.WebSocket;
using TenBot.Enums;
using TenBot.Helpers;
using TenBot.Models;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.ManageChannels)]
[Group("project", "A command to set up and manage projects.")]
public sealed class ProjectCommand : InteractionModuleBase
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;


    public ProjectCommand(DiscordSocketClient client, DiscordServerSettings serverSettings)
    {
        Client = client;
        ServerSettings = serverSettings;
    }

    [SlashCommand("create", "Creates a new project.")]
    public async Task CreateAsync([Summary("name", "The project's name.")] string name,
                                  [Summary("type", "The project pattern, which should be used.")] ProjectType type)
    {
        var projectTemplate = ProjectTemplateMapper.GetProjectTemplateFromProjectType(type);
        var server = Client.GetGuild(ServerSettings.GuildID);

        var role = await server.CreateRoleAsync($"{name} - Project", isMentionable: true);
        var category = await server.CreateCategoryChannelAsync(name);

        await RespondAsync($"{type} project '{name}' was successfully created.", ephemeral: true);

        await SetProjectChannelPermissionsAsync(server, role, category);

        foreach (var channel in projectTemplate.Channels) await CreateProjectChannelAsync(server, role, channel, category);
        await (Context.User as IGuildUser)!.AddRoleAsync(role);
    }

    [SlashCommand("delete", "Deletes an existing project.")]
    public async Task DeleteAsync([Summary("name", "The project's name."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string name)
    {
        var category = Client.GetGuild(ServerSettings.GuildID).CategoryChannels.First(x => x.PermissionOverwrites.Any(x => Client.GetGuild(ServerSettings.GuildID).Roles.Any(y => y.Name.EndsWith(" - Project") && y.Id == x.TargetId)));
        var role = Client.GetGuild(ServerSettings.GuildID).Roles.First(x => x.Name.EndsWith(" - Project") && category.PermissionOverwrites.Any(y => y.TargetId == x.Id));

        foreach (var channel in category.Channels) await channel.DeleteAsync();
        await category.DeleteAsync();
        await role.DeleteAsync();

        await RespondAsync($"Project '{category.Name}' was successfully deleted.", ephemeral: true);
    }

    [SlashCommand("invite", "Invites another person into a project.")]
    public async Task InviteAsync([Summary("project", "The project to invite somebody in."),
                                   Autocomplete(typeof(ProjectAutoCompleteHandler))] string project,
                                  IGuildUser user)
    {
        var category = Client.GetGuild(ServerSettings.GuildID).CategoryChannels.First(x => x.PermissionOverwrites.Any(x => Client.GetGuild(ServerSettings.GuildID).Roles.Any(y => y.Name.EndsWith(" - Project") && y.Id == x.TargetId)));
        var role = Client.GetGuild(ServerSettings.GuildID).Roles.First(x => x.Name.EndsWith(" - Project") && category.PermissionOverwrites.Any(y => y.TargetId == x.Id));

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
        [SlashCommand("list", "Displayes all project templates.")]
        public async Task ListAsync()
        {
            var embed = new EmbedBuilder()
                .WithColor(Color.Purple)
                .WithTitle("Available project templates");

            foreach (var projectType in Enum.GetValues(typeof(ProjectType)).Cast<ProjectType>())
            {
                var projectTemplate = ProjectTemplateMapper.GetProjectTemplateFromProjectType(projectType);
                _ = embed.AddField(new EmbedFieldBuilder()
                                   .WithName(projectType.ToString())
                                   .WithValue(projectTemplate.Description));
            }

            await RespondAsync(embeds: new Embed[] { embed.Build() }, ephemeral: true);
        }

        [SlashCommand("explain", "Explains a project template.")]
        public async Task ExplainAsync([Summary("template", "The template you want to get explained.")] ProjectType template)
        {
            var projectTemplate = ProjectTemplateMapper.GetProjectTemplateFromProjectType(template);

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
