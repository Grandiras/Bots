namespace TenBot.Models;
public sealed record DiscordServerSettings(string Token,
                                           ulong GuildID,
                                           ulong NewTalkChannelID,
                                           ulong NewPrivateTalkChannelID,
                                           ulong VoiceCategoryID,
                                           ulong MemberRoleID,
                                           bool IsRoleSelectionEnabled);
