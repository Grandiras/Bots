namespace TenBot.Features.VoiceManager;
public sealed class VoiceManagerData
{
    public required ulong VoiceCreationCategory { get; set; }
    public required ulong NewVoiceChannel { get; set; }
    public required ulong NewPrivateVoiceChannel { get; set; }

    public required ulong VoiceChannelCategory { get; set; }
    public required string DefaultVoiceChannelName { get; set; }
    public required string DefaultPrivateVoiceChannelName { get; set; }

    public required string VoiceCreationCategoryName { get; set; }
    public required string NewVoiceChannelName { get; set; }
    public required string NewPrivateVoiceChannelName { get; set; }

    public required string VoiceChannelCategoryName { get; set; }
}
