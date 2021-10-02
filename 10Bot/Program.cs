using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.Rest;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace _10Bot
{
    internal class Program
    {
        private DiscordSocketClient Client;
        private SocketGuild Guild;

        private List<string> WelcomeMessages = new List<string>();
        private Random Randomizer = new Random();

        private const ulong GUILD_ID = 834815553372946474;

        private const ulong NEW_TALK_CHANNEL_ID = 847132103576387617;
        private const ulong NEW_PRIVATE_TALK_CHANNEL_ID = 847132104617492510;

        private const ulong GENERAL_TEXTVOICE_ID = 893778409795252265;

        private const ulong VOICE_CATEGORY_ID = 847132102633062421;
        private const ulong TEXTVOICE_CATEGORY_ID = 858056836199350313;

        private List<VoiceSettings> VoiceChannels = new List<VoiceSettings>();

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();

            Client.Log += Log;
            Client.Ready += Ready;
            Client.Disconnected += Disconnected;
            Client.InteractionCreated += InteractionCreated;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            Client.UserJoined += UserJoined;

            var token = "ODkzNTExMTA1MDEwMzY0NDI2.YVchEA.vyv1d5Hc8U_WngD8XyhRfTtIRfE";

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task UserJoined(SocketGuildUser user)
        {
            await Guild.DefaultChannel.SendMessageAsync(WelcomeMessages[Randomizer.Next(0, WelcomeMessages.Count - 1)].Replace("[]", user.Username));
        }

        private async Task Disconnected(Exception arg)
        {
            await Client.StartAsync();
        }

        private async Task UserVoiceStateUpdated(SocketUser member, SocketVoiceState before, SocketVoiceState after)
        {
            if (after.VoiceChannel != null)
            {
                if (after.VoiceChannel.Id == NEW_TALK_CHANNEL_ID)
                {
                    var role = await Guild.CreateRoleAsync("Voice", isMentionable: false);
                    var generalTextVoice = Guild.GetChannel(GENERAL_TEXTVOICE_ID);

                    await generalTextVoice.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        viewChannel: PermValue.Allow));

                    var channel = await Guild.CreateVoiceChannelAsync("Voice", x => x.CategoryId = VOICE_CATEGORY_ID);

                    var textChannel = await Guild.CreateTextChannelAsync("Voice", x => x.CategoryId = TEXTVOICE_CATEGORY_ID);
                    await textChannel.AddPermissionOverwriteAsync(Guild.EveryoneRole, new OverwritePermissions(
                        viewChannel: PermValue.Deny));
                    await textChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        viewChannel: PermValue.Allow));

                    VoiceChannels.Add(new VoiceSettings(channel, textChannel, role));

                    var guildMember = member as IGuildUser;
                    await guildMember.AddRoleAsync(role);
                    await guildMember.ModifyAsync(x => x.Channel = channel);
                }
                else if (after.VoiceChannel.Id == NEW_PRIVATE_TALK_CHANNEL_ID)
                {
                    var role = await Guild.CreateRoleAsync("Private Voice", isMentionable: false);
                    var generalTextVoice = Guild.GetChannel(GENERAL_TEXTVOICE_ID);

                    await generalTextVoice.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        viewChannel: PermValue.Allow));

                    var channel = await Guild.CreateVoiceChannelAsync("Private Voice", x => x.CategoryId = VOICE_CATEGORY_ID);
                    await channel.AddPermissionOverwriteAsync(Guild.EveryoneRole, new OverwritePermissions(
                        viewChannel: PermValue.Deny));
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        viewChannel: PermValue.Allow));

                    var textChannel = await Guild.CreateTextChannelAsync("Private Voice", x => x.CategoryId = TEXTVOICE_CATEGORY_ID);
                    await textChannel.AddPermissionOverwriteAsync(Guild.EveryoneRole, new OverwritePermissions(
                        viewChannel: PermValue.Deny));
                    await textChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        viewChannel: PermValue.Allow));

                    VoiceChannels.Add(new PrivateVoiceSettings(channel, textChannel, role, member));

                    var guildMember = member as IGuildUser;
                    await guildMember.AddRoleAsync(role);
                    await guildMember.ModifyAsync(x => x.Channel = channel);
                }
                else if (VoiceChannels.Where(x => x.Channel.Id == after.VoiceChannel.Id).FirstOrDefault() != null)
                {
                    var guildMember = member as IGuildUser;
                    await guildMember.AddRoleAsync(VoiceChannels.Where(x => x.Channel.Id == after.VoiceChannel.Id).FirstOrDefault().Role);
                }
            }

            if (before.VoiceChannel != null)
            {
                if (VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault() != null)
                {
                    var channel = VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault();
                    var privateChannel = channel as PrivateVoiceSettings;
                    if (privateChannel == null)
                    {
                        var guildMember = member as IGuildUser;
                        await guildMember.RemoveRoleAsync(channel.Role);
                    }
                }

                if (!(before.VoiceChannel.Id == NEW_TALK_CHANNEL_ID) && before.VoiceChannel.Users.Count == 0 && VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault() != null)
                {
                    var channel = VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault();
                    await channel.Role.DeleteAsync();
                    await channel.Channel.DeleteAsync();
                    await channel.TextChannel.DeleteAsync();
                    VoiceChannels.Remove(channel);
                }
            }
        }

        private async Task InteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                await HandlerSlashCommands.FindCommand(command);
            }
        }

        private async Task Ready()
        {
            /*var guildCommand = new SlashCommandBuilder()
                                   .WithName("list-roles")
                                   .WithDescription("Lists all roles of user.")
                                   .AddOption("user", ApplicationCommandOptionType.User, "ther user whos roles you want to be listed", required: true);

            try
            {
                await Client.Rest.CreateGuildCommand(guildCommand.Build(), GUILD_ID);
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }*/

            Guild = Client.GetGuild(GUILD_ID);
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.Delay(1);
        }
    }
}
