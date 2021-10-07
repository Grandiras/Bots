using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
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
        internal SocketGuild Guild;
        private HandleSlashCommands CommandHandler;

        private List<string> WelcomeMessages; 
        private Random Randomizer = new Random();

        internal List<CustomCommand> CustomCommands;

        private List<VoiceSettings> VoiceChannels = new List<VoiceSettings>();

        internal static Program Instance { get; private set; }
        #endregion
        // -----
        #region Consts
        public ulong GuildID { get; private set; }

        public ulong NewTalkChannelID { get; private set; }
        public ulong NewPrivateTalkChannelID { get; private set; }

        public ulong GeneralTextVoiceID { get; private set; }

        public ulong VoiceCategoryID { get; private set; }
        public ulong TextVoiceCategoryID { get; private set; }

        public ulong ModeratorRoleID { get; private set; }
        public ulong MemberRoleID { get; private set; }

        public string Token { get; private set; } = "";
        #endregion
        // -----
        #region Main
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Instance = this;

            Client = new DiscordSocketClient();
            CommandHandler = new HandleSlashCommands(VoiceChannels);

            #region GetConfigData
            string configJson = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/config.json");
            var config = JsonConvert.DeserializeObject<Dictionary<string, object>>(configJson);
            foreach (var item in config)
            {
                Type type = GetType();
                PropertyInfo propInfo = type.GetProperty(item.Key);
                var propTypeValue = Convert.ChangeType(item.Value, propInfo.PropertyType); 
                propInfo.SetValue(this, propTypeValue);
            }
            #endregion

            string json = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/welcome_messages.json");
            WelcomeMessages = JsonConvert.DeserializeObject<List<string>>(json);

            string json2 = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/custom_commands.json");
            CustomCommands = JsonConvert.DeserializeObject<List<CustomCommand>>(json2);
            
            if (CustomCommands == null)
            {
                CustomCommands = new List<CustomCommand>();   
            }

            #region Events + Handlers
            Client.Log += Log;
            Client.Ready += Ready;
            Client.Disconnected += Disconnected;
            Client.InteractionCreated += InteractionCreated;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            Client.UserJoined += UserJoined;
            #endregion

            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            var isConnecting = true;

            while (true)
            {
                Thread.Sleep(1000 * 1/60);

                if(Client.ConnectionState == ConnectionState.Disconnecting) isConnecting = false;

                if (isConnecting == false && Client.ConnectionState == ConnectionState.Disconnected)
                {
                    using (var file = File.Create(Directory.GetCurrentDirectory() + $"/Logs/{DateTime.Now.ToString().Replace(" ", "_").Replace(".", "-").Replace(":", "-")}.txt"))
                    {
                        TextWriter writer = new StreamWriter(file);
                        Console.SetOut(writer);
                        writer.Close();
                    }

                    Main(new string[0]);
                    isConnecting = true;

                    return;
                }
            }
        }
        #endregion
        // -----
        private async Task UserJoined(SocketGuildUser user)
        {
            await Guild.DefaultChannel.SendMessageAsync(WelcomeMessages[Randomizer.Next(0, WelcomeMessages.Count - 1)].Replace("[]", user.Username));
        }
        // -----
        #region Disconnected
        private async Task Disconnected(Exception ex)
        {
            var json = JsonConvert.SerializeObject(ex.Message, Formatting.Indented);
            Console.WriteLine(json);

            using (var file = File.Create(Directory.GetCurrentDirectory() + $"/Logs/{DateTime.Now}.txt"))
            {
                TextWriter writer = new StreamWriter(file);
                Console.SetOut(writer);
                writer.Close();
            }
        }
        #endregion
        // -----
        #region UserVoiceStateUpdated
        private async Task UserVoiceStateUpdated(SocketUser member, SocketVoiceState before, SocketVoiceState after)
        {
            if (after.VoiceChannel != null)
            {
                if (after.VoiceChannel.Id == NewTalkChannelID)
                {
                    var role = await Guild.CreateRoleAsync("Voice", isMentionable: false);
                    var generalTextVoice = Guild.GetChannel(GeneralTextVoiceID);

                    await generalTextVoice.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        viewChannel: PermValue.Allow));

                    var channel = await Guild.CreateVoiceChannelAsync("Voice", x => x.CategoryId = VoiceCategoryID);

                    var textChannel = await Guild.CreateTextChannelAsync("Voice", x => x.CategoryId = TextVoiceCategoryID);
                    await textChannel.AddPermissionOverwriteAsync(Guild.EveryoneRole, new OverwritePermissions(
                        viewChannel: PermValue.Deny));
                    await textChannel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        viewChannel: PermValue.Allow));

                    VoiceChannels.Add(new VoiceSettings(channel, textChannel, role));

                    var guildMember = member as IGuildUser;
                    await guildMember.AddRoleAsync(role);
                    await guildMember.ModifyAsync(x => x.Channel = channel);
                }
                else if (after.VoiceChannel.Id == NewPrivateTalkChannelID)
                {
                    var role = await Guild.CreateRoleAsync("Private Voice", isMentionable: false);
                    var generalTextVoice = Guild.GetChannel(GeneralTextVoiceID);

                    await generalTextVoice.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        readMessageHistory: PermValue.Allow,
                        sendMessages: PermValue.Allow,
                        viewChannel: PermValue.Allow));

                    var channel = await Guild.CreateVoiceChannelAsync("Private Voice", x => x.CategoryId = VoiceCategoryID);
                    await channel.AddPermissionOverwriteAsync(Guild.EveryoneRole, new OverwritePermissions(
                        viewChannel: PermValue.Deny));
                    await channel.AddPermissionOverwriteAsync(role, new OverwritePermissions(
                        viewChannel: PermValue.Allow));

                    var textChannel = await Guild.CreateTextChannelAsync("Private Voice", x => x.CategoryId = TextVoiceCategoryID);
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

                if (!(before.VoiceChannel.Id == NewTalkChannelID) && before.VoiceChannel.Users.Count == 0 && VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault() != null)
                {
                    var channel = VoiceChannels.Where(x => x.Channel.Id == before.VoiceChannel.Id).FirstOrDefault();
                    await channel.Role.DeleteAsync();
                    await channel.Channel.DeleteAsync();
                    await channel.TextChannel.DeleteAsync();
                    VoiceChannels.Remove(channel);
                }
            }
        }
        #endregion
        // -----
        #region InteractionCreated
        private async Task InteractionCreated(SocketInteraction arg)
        {
            if (arg is SocketSlashCommand command)
            {
                await CommandHandler.FindCommandAsync(command);
            }
        }
        #endregion
        // -----
        #region Ready
        private async Task Ready()
        {
            List<SlashCommandBuilder> commands = new List<SlashCommandBuilder>
            {
                #region System Commands
                 new SlashCommandBuilder()
                    .WithName("invite")
                    .WithDescription("Lädt einen Nutzer in den Private Talk ein.")
                    .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, der eingeladen werden soll", required: true),

                new SlashCommandBuilder()
                    .WithName("kick")
                    .WithDescription("Kickt einen Nutzer aus deinem Private Talk.")
                    .AddOption("user", ApplicationCommandOptionType.User, "Der Nutzer, der gekickt werden soll", required: true)
                    .AddOption("reason", ApplicationCommandOptionType.String, "Grund für den Kick", required: false),

                new SlashCommandBuilder()
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
                        .WithType(ApplicationCommandOptionType.SubCommand)),

                new SlashCommandBuilder()
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
                        .WithType(ApplicationCommandOptionType.SubCommand)),

                new SlashCommandBuilder()
                    .WithName("channel")
                    .WithDescription("Ein Command zur Channel-Verwaltung.")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("rename")
                        .WithDescription("Benennt deinen aktuellen Channel um.")
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, "Der neue Name des Channels", required: true)),

                new SlashCommandBuilder()
                    .WithName("command")
                    .WithDescription("Ein Command zur Custom Command Verwaltung.")
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("create")
                        .WithDescription("Erstellt einen neuen Command.")
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
                        .AddOption("text", ApplicationCommandOptionType.String, "Der neue Text", required: true)),

                new SlashCommandBuilder()
                    .WithName("accept")
                    .WithDescription("Damit kannst du die Regeln akzeptieren."),

                new SlashCommandBuilder()
                    .WithName("help")
                    .WithDescription("Zeigt alle verfügbaren Commands an.")
                #endregion  
            };

            try
            {
                foreach (var item in commands)
                {
                    await Client.Rest.CreateGuildCommand(item.Build(), GuildID);
                }

                await CreateCustomCommands();
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }

            Guild = Client.GetGuild(GuildID);
        }
        #endregion
        // -----
        #region Log
        private async Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            await Task.Delay(1);
        }
        #endregion
        // -----
        #region CreateCustomCommands
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
                await Client.Rest.CreateGuildCommand(executeCommand.Build(), GuildID);
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }

            using (TextWriter file = File.CreateText(Directory.GetCurrentDirectory() + "/Data/custom_commands.json"))
            {
                JsonSerializer serializer = new JsonSerializer();
                serializer.Serialize(file, CustomCommands);
            }
        }
        #endregion
    }
}
