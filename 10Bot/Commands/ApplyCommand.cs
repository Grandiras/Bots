using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.Models;
using TenBot.Services;

namespace TenBot.Commands;
[Group("apply", "Apply for special positions with this command!")]
public sealed class ApplyCommand : InteractionModuleBase
{
    private readonly ServerService ServerService;
    private readonly DiscordServerSettingsStorage ServerSettings;


    public ApplyCommand(ServerService serverService, DiscordServerSettingsStorage serverSettings)
    {
        ServerService = serverService;
        ServerSettings = serverSettings;
    }


    [SlashCommand("moderator", "Apply for the role of a moderator!")]
    public async Task ModeratorAsync()
    {
        if ((Context.User as SocketGuildUser)!.GuildPermissions.ManageMessages)
        {
            await RespondAsync("You are already a moderator!", ephemeral: true);
            return;
        }

        await RespondWithModalAsync<ApplicationModal>($"{Context.User.Username}_application");
    }

    [ModalInteraction("*_application", true)]
    public async Task ApplicationSubmittedAsync(string userName, ApplicationModal modal)
    {
        var server = ServerSettings.Settings[Context.Guild.Id];

        var embed = new EmbedBuilder()
            .WithTitle("Application submitted")
            .WithColor(199, 62, 38)
            .AddField(new EmbedFieldBuilder()
                .WithName("By")
                .WithValue(Context.User.Username))
            .AddField(new EmbedFieldBuilder()
                .WithName("Content")
                .WithValue(modal.Reason));

        _ = await ServerService.GetServer(server.GuildID).PublicUpdatesChannel.SendMessageAsync(embed: embed.Build());
        await RespondAsync("Your application has successfully been redirected to the team.", ephemeral: true);
    }
}
