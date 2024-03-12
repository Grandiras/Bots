using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using TenBot.ServerAbstractions;

namespace TenBot.Features.VoiceManager;

[Group("channel", "Manages voice channels dynamically."), DefaultMemberPermissions(GuildPermission.Connect)]
public sealed class VoiceManagerCommand(VoiceManagerService VoiceManagerService) : InteractionModuleBase<ServerInteractionContext>
{
    [SlashCommand("rename", "Renames your current channel.")]
    public async Task RenameAsync([Summary("newName", "The new name for your channel.")] string newName)
    {
        if (Context.User.VoiceChannel is null)
        {
            await RespondAsync("You are not in a voice channel.", ephemeral: true);
            return;
        }

        if (!VoiceManagerService.IsInCreatedVoiceChannel(Context.User))
        {
            await RespondAsync("You are not in a dynamically created voice channel.", ephemeral: true);
            return;
        }

        if (newName.Length > 30)
        {
            await RespondAsync("The given name is too long!", ephemeral: true);
            return;
        }

        // TODO safe guards
        if (!(newName == Context.User.VoiceChannel.Name)) await Context.User.VoiceChannel.ModifyAsync(x => x.Name = newName);

        await RespondAsync($"Your channel was renamed to '{newName}'.", ephemeral: true);
    }

    [SlashCommand("invite", "Invites someone to your private channel."), UserCommand("Invite to talk")]
    public async Task InviteAsync([Summary("user", "The user to invite.")] SocketGuildUser user)
    {
        if (Context.User.VoiceChannel is null)
        {
            await RespondAsync("You are not in a voice channel.", ephemeral: true);
            return;
        }

        var role = GetPrivateChannelRole(Context.User.VoiceChannel);

        if (!VoiceManagerService.IsInCreatedVoiceChannel(Context.User) || role is null)
        {
            await RespondAsync("You are not in a dynamically created and private voice channel.", ephemeral: true);
            return;
        }

        if (!user.Roles.Any(x => x.Id == role.Id)) await user.AddRoleAsync(role);
        await RespondAsync($"{user.Mention} was added to this channel.", ephemeral: true);
    }

    [SlashCommand("invite-role", "Invites everyone with the given role to your private channel.")]
    public async Task InviteRoleAsync([Summary("role", "The role to invite.")] SocketRole role)
    {
        if (Context.User.VoiceChannel is null)
        {
            await RespondAsync("You are not in a voice channel.", ephemeral: true);
            return;
        }

        var voiceRole = GetPrivateChannelRole(Context.User.VoiceChannel);

        if (!VoiceManagerService.IsInCreatedVoiceChannel(Context.User) || voiceRole is null)
        {
            await RespondAsync("You are not in a dynamically created and private voice channel.", ephemeral: true);
            return;
        }

        // TODO bug: not working
        if (role.Id != voiceRole.Id) foreach (var user in role.Members) if (!user.Roles.Any(x => x.Id == voiceRole.Id)) _ = user.AddRoleAsync(voiceRole);
        await RespondAsync($"{role.Mention} was added to this channel.", ephemeral: true);
    }

    [SlashCommand("is-private", "Tells you, whether you current channel is public or private.")]
    public async Task IsPrivateAsync()
        => await RespondAsync($"Your current channel is {(Context.User.VoiceChannel.PermissionOverwrites.Any(x => x.Permissions.ViewChannel == PermValue.Allow) ? "private" : "public")}.", ephemeral: true);

    [SlashCommand("convert", "Converts your current voice channel between public and private.")]
    public async Task ConvertToAsync()
    {
        if (Context.User.VoiceChannel is null)
        {
            await RespondAsync("You are not in a voice channel.", ephemeral: true);
            return;
        }

        if (!VoiceManagerService.IsInCreatedVoiceChannel(Context.User))
        {
            await RespondAsync("You are not in a dynamically created voice channel.", ephemeral: true);
            return;
        }

        var role = GetPrivateChannelRole(Context.User.VoiceChannel);

        if (role is not null)
        {
            await role.DeleteAsync();
            await Context.User.VoiceChannel.RemovePermissionOverwriteAsync(Context.Guild.EveryoneRole);

            await RespondAsync("Successfully converted your current voice channel to public.", ephemeral: true);
            return;
        }

        var newRole = await Context.Guild.CreateRoleAsync(Context.User.VoiceChannel.Name, isMentionable: false);

        await Context.User.VoiceChannel.AddPermissionOverwriteAsync(Context.Guild.EveryoneRole, new OverwritePermissions(viewChannel: PermValue.Deny));
        await Context.User.VoiceChannel.AddPermissionOverwriteAsync(newRole, new OverwritePermissions(viewChannel: PermValue.Allow));

        // TODO bug: bot gets the role too
        foreach (var user in Context.User.VoiceChannel.Users) _ = user.AddRoleAsync(newRole);
        await RespondAsync("Successfully converted your current voice channel to private.", ephemeral: true);
    }

    [ModalInteraction(nameof(VoiceManagerSetupModal), true)]
    public async Task ModalResponseAsync(VoiceManagerSetupModal modal)
    {
        _ = Context.AddFeatureAsync(VoiceManagerService.Feature);
        _ = VoiceManagerService.AddServerAsync(Context.ServerID, modal);
        await RespondAsync($"Feature was successfully configured.", ephemeral: true);
    }

    private SocketRole? GetPrivateChannelRole(SocketVoiceChannel channel)
        => channel.PermissionOverwrites.Where(x => x.Permissions.ViewChannel == PermValue.Allow) is IEnumerable<Overwrite> overwrites && overwrites.Any()
            ? Context.Guild.Roles.First(x => x.Id == overwrites.First().TargetId)
            : null;
}