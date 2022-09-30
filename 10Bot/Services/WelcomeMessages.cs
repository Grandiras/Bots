using Discord;
using Newtonsoft.Json;

namespace TenBot.Services;
public sealed class WelcomeMessages
{
    public List<string> Messages { get; }
    public Random Randomizer { get; } = new();


    public WelcomeMessages()
    {
        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/welcome_messages.json");
        Messages = JsonConvert.DeserializeObject<List<string>>(json)!;
    }


    public string GetWelcomeMessage(IGuildUser user)
        => Messages[Randomizer.Next(0, Messages.Count - 1)].Replace("[]", user.Mention);
}
