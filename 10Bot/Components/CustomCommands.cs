using Newtonsoft.Json;
using Discord.WebSocket;

namespace TenBot.Components;
internal class CustomCommands : IDisposable
{
    private readonly DiscordSocketClient Client;
    private readonly DiscordServerSettings ServerSettings;

    //public List<CustomCommand> Commands { get; }


    public CustomCommands(DiscordSocketClient client, DiscordServerSettings serverSettings)
    {
        Client = client;
        ServerSettings = serverSettings;

        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/custom_commands.json");
        //Commands = JsonConvert.DeserializeObject<List<CustomCommand>>(json)!;
    }


    public void Dispose() => throw new NotImplementedException();


    public async Task CreateCustomCommandsAsync()
    {
        //var executeCommand = new SlashCommandBuilder()
        //        .WithName("execute")
        //        .WithDescription(CommandHandler.LanguageTokens["execute_description"]);

        //foreach (var item in Commands)
        //{
        //    _ = executeCommand
        //        .AddOption(new SlashCommandOptionBuilder()
        //        .WithName(item.Name)
        //        .WithDescription(item.Description)
        //        .WithType(ApplicationCommandOptionType.SubCommand));
        //}

        //try
        //{
        //    _ = await Client.Rest.CreateGuildCommand(executeCommand.Build(), ServerSettings.GuildID);
        //}
        //catch (HttpException ex)
        //{
        //    var json = JsonConvert.SerializeObject(ex.Errors, Formatting.Indented);
        //    Console.WriteLine(json);
        //}

        //using TextWriter file = File.CreateText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/custom_commands.json");
        //var serializer = new JsonSerializer();
        //serializer.Serialize(file, Commands);
    }
}
