using Discord;
using Discord.Interactions;
using TenBot.Services;

namespace TenBot.Commands;
[DefaultMemberPermissions(GuildPermission.ManageGuild)]
[Group("welcome_message", "Used to create and manage custom welcome messages.")]
public sealed class WelcomeMessageCommand : InteractionModuleBase
{
	private readonly WelcomeMessages WelcomeMessages;


	public WelcomeMessageCommand(WelcomeMessages welcomeMessages) => WelcomeMessages = welcomeMessages;


    [SlashCommand("create", "Creates a new custom welcome message.")]
    public async Task CreateAsync([Summary("message", "The message to be displayed on joining. Use square brackets to declare space for the username.")] string message)
    {
        WelcomeMessages.AddWelcomeMessage(message, Context.Guild.Id);
        await RespondAsync("Message has been successfully added.", ephemeral: true);
    }

	[SlashCommand("list", "Displays all available custom welcome messages.")]
	public async Task ListAsync()
	{
		var embed = new EmbedBuilder()
			.WithTitle("Custom welcome messages")
			.WithColor(Color.Teal);

		foreach (var message in WelcomeMessages.GetWelcomeMessages(Context.Guild.Id))
			_ = embed.AddField(new EmbedFieldBuilder()
						   .WithName("Welcome message")
						   .WithValue(message));

        await RespondAsync(embed: embed.Build(), ephemeral: true);
    }
}
