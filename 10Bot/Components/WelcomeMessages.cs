using Discord.WebSocket;
using Newtonsoft.Json;

namespace TenBot.Components;
internal class WelcomeMessages : IDisposable
{
    private readonly SocketGuild Server;
    
    private readonly List<string> Messages;
    private readonly Random Randomizer;

    public WelcomeMessages(SocketGuild server)
    {
        Server = server;
        
        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/welcome_messages.json");
        Messages = JsonConvert.DeserializeObject<List<string>>(json)!;
        Randomizer = new();
    }


    public void Dispose() => throw new NotImplementedException();


    public async Task UserJoinedAsync(SocketGuildUser user) 
        => _ = await Server.DefaultChannel.SendMessageAsync(Messages[Randomizer.Next(0, Messages.Count - 1)]
                                                                               .Replace("[]", user.Username));
}
