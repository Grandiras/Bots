using Discord;
using Discord.Interactions;
using TenBot.ServerAbstractions;

namespace TenBot.Features.WelcomeMessages;

[Group("welcome-messages", "Sends a welcome message to new users."), DefaultMemberPermissions(GuildPermission.Administrator)]
public sealed class WelcomeMessagesCommand : InteractionModuleBase<ServerInteractionContext>
{
    private readonly WelcomeMessagesService WelcomeMessagesService;


    public WelcomeMessagesCommand(WelcomeMessagesService welcomeMessagesService) => WelcomeMessagesService = welcomeMessagesService;


    [SlashCommand("add", "Adds a new welcome message to your server.")]
    public async Task AddAsync([Summary("message", "The message to add. Use '[]' as a placeholder for a mention of the user.")] string message)
    {
        await WelcomeMessagesService.AddMessage(Context.ServerID, message);
        await RespondAsync($"The message '{message}' was successfully added.", ephemeral: true);
    }

    [SlashCommand("remove", "Removes a welcome message from your server.")]
    public async Task RemoveAsync([Summary("message", "The message to remove."), Autocomplete(typeof(WelcomeMessagesAutoCompleteHandler))] string message)
    {
        await WelcomeMessagesService.RemoveMessage(Context.ServerID, message);
        await RespondAsync($"The message '{message}' was successfully removed.", ephemeral: true);
    }

    [SlashCommand("list", "Lists all welcome messages on your server.")]
    public async Task ListAsync()
    {
        var messages = WelcomeMessagesService.GetMessages(Context.ServerID);

        var embed = new EmbedBuilder()
            .WithTitle("Welcome Messages")
            .WithColor(WelcomeMessagesService.Feature.Color)
            .WithDescription("These are all welcome messages that are currently available on this server.");

        foreach (var message in messages) _ = embed.AddField(message, $"{message.Replace("[]", Context.User.Mention)}");

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }

    [ModalInteraction(nameof(WelcomeMessagesSetupModal), true)]
    public async Task ModalResponseAsync(WelcomeMessagesSetupModal modal)
    {
        var channel = Context.HasChannel(modal.Channel, ChannelType.Text);

        if (channel.IsT1)
        {
            await RespondAsync("Channel not found. Please rerun the initialization process using\n```/feature init```", ephemeral: true);
            return;
        }

        _ = Context.AddFeatureAsync(WelcomeMessagesService.Feature);
        _ = WelcomeMessagesService.AddServer(Context.ServerID, channel.AsT0);
        await RespondAsync($"Feature was successfully configured. The channel was set to '{modal.Channel}'.", ephemeral: true);
    }
}
