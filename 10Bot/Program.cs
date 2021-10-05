using System;
using System.Collections.Generic;
using System.IO;
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
        #region Fields
        private DiscordSocketClient Client;
        private SocketGuild Guild;
        private HandleSlashCommands CommandHandler;

        private List<string> WelcomeMessages; 
        private Random Randomizer = new Random();

        internal List<CustomCommand> CustomCommands;

        private List<VoiceSettings> VoiceChannels = new List<VoiceSettings>();

        internal static Program Instance { get; private set; }
        #endregion
        // -----
        #region Consts
        private const ulong GUILD_ID = 835862190640201728;

        private const ulong NEW_TALK_CHANNEL_ID = 845321570834579497;
        private const ulong NEW_PRIVATE_TALK_CHANNEL_ID = 845321649117331496;

        private const ulong GENERAL_TEXTVOICE_ID = 855753678941585438;

        private const ulong VOICE_CATEGORY_ID = 835862190640201731;
        private const ulong TEXTVOICE_CATEGORY_ID = 855754084628168704;

        internal const ulong MODERATOR_ROLE_ID = 845313134344274001;

        private const string TOKEN = "ODkzNTExMTA1MDEwMzY0NDI2.YVchEA.vyv1d5Hc8U_WngD8XyhRfTtIRfE";
        #endregion
        // -----
        #region Main
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Instance = this;

            Client = new DiscordSocketClient();
            CommandHandler = new HandleSlashCommands(VoiceChannels);

            string json = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/welcome_messages.json");
            WelcomeMessages = JsonConvert.DeserializeObject<List<string>>(json);

            string json2 = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/custom_commands.json");
            CustomCommands = JsonConvert.DeserializeObject<List<CustomCommand>>(json2);
            
            if (CustomCommands == null)
            {
                CustomCommands = new List<CustomCommand>();   
            }

            Client.Log += Log;
            Client.Ready += Ready;
            Client.Disconnected += Disconnected;
            Client.InteractionCreated += InteractionCreated;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            Client.UserJoined += UserJoined;

            await Client.LoginAsync(TokenType.Bot, TOKEN);
            await Client.StartAsync();

            await Task.Delay(-1);
        }
        #endregion
        // -----
        private async Task UserJoined(SocketGuildUser user)
        {
            await Guild.DefaultChannel.SendMessageAsync(WelcomeMessages[Randomizer.Next(0, WelcomeMessages.Count - 1)].Replace("[]", user.Username));
        }

        private async Task Disconnected(Exception ex)
        {
            var json = JsonConvert.SerializeObject(ex.Message, Formatting.Indented);
            Console.WriteLine(json);

            await Client.StopAsync();

            await Client.LoginAsync(TokenType.Bot, TOKEN);
            await Client.StartAsync();

            using (var file = File.Create(Directory.GetCurrentDirectory() + $"/Logs/{DateTime.Now}.txt"))
            {
                TextWriter writer = new StreamWriter(file);
                Console.SetOut(writer);
                writer.Close();
            }

            using (StreamWriter file = File.CreateText(Directory.GetCurrentDirectory() + "/Data/custom_commands.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, CustomCommands);
            }
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

                    VoiceChannels.Add(new PrivateVoiceSettings(channel, textChannel, role, member as IGuildUser));

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
                await CommandHandler.FindCommandAsync(command);
            }
        }

        private async Task Ready()
        {
            var inviteCommand = new SlashCommandBuilder()
                .WithName("invite")
                .WithDescription("Lädt einen Nutzer in den Private Talk ein.")
                .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, der eingeladen werden soll", required: true);

            var kickCommand = new SlashCommandBuilder()
                .WithName("kick")
                .WithDescription("Kickt einen Nutzer aus deinem Private Talk.")
                .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, der gekickt werden soll", required: true)
                .AddOption("reason", ApplicationCommandOptionType.String, "Grund für den Kick", required: false);

            var managerCommand = new SlashCommandBuilder()
                .WithName("manager")
                .WithDescription("Ein Command zur Mod-Verwaltung.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("add")
                    .WithDescription("Fügt einen Nutzer als Mod hinzu.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, der Mod werden soll", required: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("remove")
                    .WithDescription("Entfernt einen Nutzer als Mod.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("user", ApplicationCommandOptionType.User, "Der nutzer, der kein Mod mehr sein soll", required: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("get")
                    .WithDescription("Gibt die Modliste deines Channels aus.")
                    .WithType(ApplicationCommandOptionType.SubCommand));

            var ownerCommand = new SlashCommandBuilder()
                .WithName("owner")
                .WithDescription("Ein Command zur Owner-Verwaltung.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("transfer")
                    .WithDescription("Überträgt einem Nutzer das Ownership.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, an den das Ownership übertragen werden soll", required: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("get")
                    .WithDescription("Gibt den aktuellen Owner zurück.")
                    .WithType(ApplicationCommandOptionType.SubCommand));

            var channelCommand = new SlashCommandBuilder()
                .WithName("channel")
                .WithDescription("Ein Command zur Channel-Verwaltung.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("rename")
                    .WithDescription("Benennt deinen aktuellen Channel um.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "Der neue Name des Channels", required: true));

            var commandCommand = new SlashCommandBuilder()
                .WithName("command")
                .WithDescription("Ein Command zur Custom Command Verwaltung.")
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("create")
                    .WithDescription("Erstellt einen neuen Command")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "Der Name des Commands", required: true)
                    .AddOption("text", ApplicationCommandOptionType.String, "Der Text, den der Command ausgeben soll", required: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("delete")
                    .WithDescription("Entfernt einen Custom Command.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "Der Name des zu löschenden Commands", required: true))
                .AddOption(new SlashCommandOptionBuilder()
                    .WithName("modify")
                    .WithDescription("Verändert den Text eines Custom Commands.")
                    .WithType(ApplicationCommandOptionType.SubCommand)
                    .AddOption("name", ApplicationCommandOptionType.String, "Der zu modifizierende Command", required: true)
                    .AddOption("text", ApplicationCommandOptionType.String, "Der neue Text", required: true));

            try
            {
                await Client.Rest.CreateGuildCommand(inviteCommand.Build(), GUILD_ID);
                await Client.Rest.CreateGuildCommand(kickCommand.Build(), GUILD_ID);
                await Client.Rest.CreateGuildCommand(managerCommand.Build(), GUILD_ID);
                await Client.Rest.CreateGuildCommand(ownerCommand.Build(), GUILD_ID);
                await Client.Rest.CreateGuildCommand(channelCommand.Build(), GUILD_ID);
                await Client.Rest.CreateGuildCommand(commandCommand.Build(), GUILD_ID);

                await CreateCustomCommands();
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }

            Guild = Client.GetGuild(GUILD_ID);
        }

        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.Delay(1);
        }

        public async Task CreateCustomCommands()
        {
            var executeCommand = new SlashCommandBuilder()
                    .WithName("execute")
                    .WithDescription("Ein Command zum Ausführen von Custom Commands :)");

            foreach (var item in CustomCommands)
            {
                executeCommand.AddOption(new SlashCommandOptionBuilder()
                    .WithName(item.Name)
                    .WithDescription(item.Description)
                    .WithType(ApplicationCommandOptionType.SubCommand));
            }

            try
            {
                await Client.Rest.CreateGuildCommand(executeCommand.Build(), GUILD_ID);

                await CreateCustomCommands();
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }
    }
}
