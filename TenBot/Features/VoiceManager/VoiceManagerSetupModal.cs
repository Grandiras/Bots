using Discord.Interactions;

namespace TenBot.Features.VoiceManager;

public class VoiceManagerSetupModal : IModal
{
    public string Title => "Voice Manager Setup";

    [InputLabel("Voice Creation Category"), ModalTextInput("CC", placeholder: "Category Name (new or existing)")]
    public string VoiceCreationCategory { get; set; } = "";

    [InputLabel("New Voice Channel"), ModalTextInput("newVC", placeholder: "Channel Name (new or existing)")]
    public string NewVoiceChannel { get; set; } = "";

    [InputLabel("New Private Voice Channel"), ModalTextInput("newPVC", placeholder: "Channel Name (new or existing)")]
    public string NewPrivateVoiceChannel { get; set; } = "";

    [InputLabel("Voice Channel Category"), ModalTextInput("VCC", placeholder: "Category Name (new or existing)")]
    public string VoiceChannelCategory { get; set; } = "";

    [InputLabel("(Private) Voice Channel Name"), ModalTextInput("VCN", placeholder: "Default Channel Names (separate by comma)", maxLength: 60)]
    public string DefaultVoiceChannelNames { get; set; } = "";
}
