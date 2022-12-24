using Discord;
using Discord.Interactions;

namespace TenBot.Models;
public sealed class ApplicationModal : IModal
{
    public string Title => "Application";

    [RequiredInput(true)]
    [InputLabel("Why should we accept your application?")]
    [ModalTextInput("application_reason", TextInputStyle.Paragraph, "Write here", minLength: 30)]
    public required string Reason { get; set; }
}
