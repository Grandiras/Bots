using Discord.Interactions;

namespace TenBot.Features.WelcomeMessages;
public sealed class WelcomeMessagesSetupModal : IModal
{
    public string Title => "Welcome Messages Setup";

    [InputLabel("Welcome Messages Channel"), ModalTextInput("channel", placeholder: "Channel Name")]
    public string Channel { get; set; } = "";
}