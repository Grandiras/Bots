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
using BotComponents;

namespace _10Bot
{
    /// <summary>
    /// Main Class of Bot and Console Application
    /// </summary>
    internal class Program
    {
        #region Properties
        /// <summary>
        /// Used Discord Client
        /// </summary>
        private DiscordSocketClient Client { get; set; }
        /// <summary>
        /// Current Used Guild
        /// </summary>
        internal SocketGuild Guild { get; set; }
        /// <summary>
        /// Class for Handling Commands (see HandleSlashCommands.cs)
        /// </summary>
        private _10BotSlashCommandHandler CommandHandler { get; set; }

        /// <summary>
        /// List of custom Welcome Messages which are read from 'welcome_messages.json' in MainAsync()
        /// </summary>
        private List<string> WelcomeMessages { get; set; }
        /// <summary>
        /// Randomizer to choose custom Welcome Message
        /// </summary>
        private Random Randomizer { get; set; } = new Random();

        /// <summary>
        /// List of custom Commands (see CustomCommand.cs)
        /// </summary>
        internal List<CustomCommand> CustomCommands { get; set; }

        /// <summary>
        /// List of Voice Channels created and managed (see VoiceSetting.cs)
        /// </summary>
        private List<VoiceSettings> VoiceChannels { get; set; } = new List<VoiceSettings>();

        /// <summary>
        /// Static Property of Instace to access from outside
        /// </summary>
        internal static Program Instance { get; private set; }
        #endregion
        // -----
        #region Consts
        /// <summary>
        /// ID of current used Guild (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong GuildID { get; private set; }

        /// <summary>
        /// ID of set NewTalkChannel (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong NewTalkChannelID { get; private set; }
        /// <summary>
        /// ID of set NewPrivateTalkChannel (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong NewPrivateTalkChannelID { get; private set; }

        /// <summary>
        /// ID of set GeneralTextVoice (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong GeneralTextVoiceID { get; private set; }

        /// <summary>
        /// ID of set VoiceCategory (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong VoiceCategoryID { get; private set; }
        /// <summary>
        /// ID of set TextVoiceCategory (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong TextVoiceCategoryID { get; private set; }

        /// <summary>
        /// ID of set ModeratorRole (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong ModeratorRoleID { get; private set; }
        /// <summary>
        /// ID of set MemberRole (read from 'config.json' in MainAsync())
        /// </summary>
        public ulong MemberRoleID { get; private set; }

        /// <summary>
        /// Token for the Bot (read from 'config.json' in MainAsync())
        /// </summary>
        public string Token { get; private set; } = "";
        #endregion
        // -----
        #region Main
        /// <summary>
        /// Main Function of Console Application, executes async One
        /// </summary>
        /// <param name="args">string params given in console</param>
        static void Main(string[] args) => new Program().MainAsync().GetAwaiter().GetResult();

        /// <summary>
        /// real Main Function of Program
        /// </summary>
        public async Task MainAsync()
        {
            // set Instance Property
            Instance = this;

            // set Client and CommandHandler Instances
            Client = new DiscordSocketClient();
            CommandHandler = new _10BotSlashCommandHandler(VoiceChannels);

            // get config data from file and write into properties
            #region GetConfigData
            string configJson = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/config.json");
            var config = JsonConvert.DeserializeObject<Dictionary<string, Dictionary<string, object>>>(configJson);
            foreach (var item in config["TestGuild"])
            {
                Type type = GetType();
                PropertyInfo propInfo = type.GetProperty(item.Key);
                var propTypeValue = Convert.ChangeType(item.Value, propInfo.PropertyType); 
                propInfo.SetValue(this, propTypeValue);
            }
            #endregion

            // get welcome messages from file and write into list
            string json = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/welcome_messages.json");
            WelcomeMessages = JsonConvert.DeserializeObject<List<string>>(json);

            // get current custom commands from file and write into lst
            string json2 = File.ReadAllText(Directory.GetCurrentDirectory() + "/Data/custom_commands.json");
            CustomCommands = JsonConvert.DeserializeObject<List<CustomCommand>>(json2);
            
            // set CustomCommands if currently null
            if (CustomCommands == null)
            {
                CustomCommands = new List<CustomCommand>();   
            }

            // set Handlers for Events
            #region Events + Handlers
            Client.Log += Log;
            Client.Ready += Ready;
            Client.Disconnected += Disconnected;
            Client.InteractionCreated += InteractionCreated;
            Client.UserVoiceStateUpdated += UserVoiceStateUpdated;
            Client.UserJoined += UserJoined;
            #endregion

            // start Bot
            await Client.LoginAsync(TokenType.Bot, Token);
            await Client.StartAsync();

            // if the Bot is currently connecting
            var isConnecting = true;

            // Auto Restart Routine
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
        #region UserJoined
        private async Task UserJoined(SocketGuildUser user)
        {
            await Guild.DefaultChannel.SendMessageAsync(WelcomeMessages[Randomizer.Next(0, WelcomeMessages.Count - 1)].Replace("[]", user.Username));
        }
        #endregion
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
            await CreateSystemCommands();
            await CreateCustomCommands();

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
        #region CreateCommands
        internal async Task CreateSystemCommands()
        {
            var languageTokens = CommandHandler.LanguageTokens;

            List<SlashCommandBuilder> commands = new List<SlashCommandBuilder>
            {
                #region System Commands
                 new SlashCommandBuilder()
                    .WithName("invite")
                    .WithDescription(languageTokens["invite_description"])
                    .AddOption("user", ApplicationCommandOptionType.User, languageTokens["invite_user"], required: true),

                new SlashCommandBuilder()
                    .WithName("kick")
                    .WithDescription(languageTokens["kick_description"])
                    .AddOption("user", ApplicationCommandOptionType.User, languageTokens["kick_user"], required: true)
                    .AddOption("reason", ApplicationCommandOptionType.String, languageTokens["kick_reason_optional"], required: false),

                new SlashCommandBuilder()
                    .WithName("manager")
                    .WithDescription(languageTokens["manager_description"])
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("add")
                        .WithDescription(languageTokens["manager_add_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("user", ApplicationCommandOptionType.User, languageTokens["manager_add_user"], required: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("remove")
                        .WithDescription(languageTokens["manager_remove_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("user", ApplicationCommandOptionType.User, languageTokens["manager_remove_user"], required: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("get")
                        .WithDescription(languageTokens["manager_get_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)),

                new SlashCommandBuilder()
                    .WithName("owner")
                    .WithDescription(languageTokens["owner_description"])
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("transfer")
                        .WithDescription(languageTokens["owner_transfer_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("user", ApplicationCommandOptionType.User, languageTokens["owner_transfer_user"], required: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("get")
                        .WithDescription(languageTokens["owner_get_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)),

                new SlashCommandBuilder()
                    .WithName("channel")
                    .WithDescription(languageTokens["channel_description"])
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("rename")
                        .WithDescription(languageTokens["channel_rename_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, languageTokens["channel_rename_newName"], required: true)),

                new SlashCommandBuilder()
                    .WithName("command")
                    .WithDescription(languageTokens["command_description"])
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("create")
                        .WithDescription(languageTokens["command_create_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, languageTokens["command_create_name"], required: true)
                        .AddOption("text", ApplicationCommandOptionType.String, languageTokens["command_create_text"], required: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("delete")
                        .WithDescription(languageTokens["command_delete_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, languageTokens["command_delete_name"], required: true))
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("modify")
                        .WithDescription(languageTokens["command_modify_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption("name", ApplicationCommandOptionType.String, languageTokens["command_modify_name"], required: true)
                        .AddOption("text", ApplicationCommandOptionType.String, languageTokens["command_modify_newText"], required: true)),

                new SlashCommandBuilder()
                    .WithName("accept")
                    .WithDescription(languageTokens["accept_description"]),

                new SlashCommandBuilder()
                    .WithName("help")
                    .WithDescription(languageTokens["help_description"]),

                new SlashCommandBuilder()
                    .WithName("settings")
                    .WithDescription(languageTokens["settings_description"])
                    .AddOption(new SlashCommandOptionBuilder()
                        .WithName("language")
                        .WithDescription(languageTokens["settings_language_description"])
                        .WithType(ApplicationCommandOptionType.SubCommand)
                        .AddOption(new SlashCommandOptionBuilder()
                            .WithName("new_language")
                            .WithDescription(languageTokens["settings_language_newLanguage"])
                            .WithType(ApplicationCommandOptionType.String)
                            .WithRequired(true)
                            .AddChoice("German", "german")
                            .AddChoice("English", "english")))
                #endregion  
            };

            try
            {
                foreach (var item in commands)
                {
                    await Client.Rest.CreateGuildCommand(item.Build(), GuildID);
                }
            }
            catch (ApplicationCommandException ex)
            {
                var json = JsonConvert.SerializeObject(ex.Error, Formatting.Indented);
                Console.WriteLine(json);
            }
        }

        public async Task CreateCustomCommands()
        {
            var executeCommand = new SlashCommandBuilder()
                    .WithName("execute")
                    .WithDescription(CommandHandler.LanguageTokens["execute_description"]);

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
