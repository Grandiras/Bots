using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Net;
using Discord.WebSocket;
using Newtonsoft.Json;

namespace _10Bot
{
    internal class Program
    {
        private DiscordSocketClient Client;
        private SocketGuild Guild;

        private const ulong GuildId = 834815553372946474;
        private const ulong NewTalkChannelId = 847132103576387617;
        private const ulong NewPrivateTalkChannelId = 847132104617492510;

        private List<SocketVoiceChannel> VoiceChannels = new List<SocketVoiceChannel>();

        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Client = new DiscordSocketClient();
            Guild = Client.Guilds.First();

            Client.Log += Log;
            Client.Ready += Ready;
            Client.InteractionCreated += InteractionCreated;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;

            var token = "ODkzNTExMTA1MDEwMzY0NDI2.YVchEA.vyv1d5Hc8U_WngD8XyhRfTtIRfE";

            await Client.LoginAsync(TokenType.Bot, token);
            await Client.StartAsync();

            await Task.Delay(-1);
        }

        private async Task UserVoiceStateUpdated(SocketUser member, SocketVoiceState before, SocketVoiceState after)
        {
            if (after.VoiceChannel != null)
            {
                if (after.VoiceChannel.Id == NewTalkChannelId)
                {

                }
                else if (after.VoiceChannel.Id == NewPrivateTalkChannelId)
                {

                }
                else if (VoiceChannels.Contains(after.VoiceChannel))
                {
                    var role = Guild.CreateRoleAsync("voice", options:null);
                }
            }
        }

        private async Task InteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                HandlerSlashCommands.FindCommand(command);
            }
        }

        private async Task Ready()
        {
            var guildCommand = new SlashCommandBuilder()
                                   .WithName("list-roles")
                                   .WithDescription("Lists all roles of user.")
                                   .AddOption("user", ApplicationCommandOptionType.User, "ther user whos roles you want to be listed", required: true);

            try
            {
                await Client.Rest.CreateGuildCommand(guildCommand.Build(), GuildId);
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
        }
    }
}
