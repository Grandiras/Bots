using Discord;
using Newtonsoft.Json;

namespace TenBot.Services;
public sealed class WelcomeMessages
{
    private readonly List<string> Messages;
    private readonly Random Randomizer = new();


    public WelcomeMessages()
    {
        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/welcome_messages.json");
        Messages = JsonConvert.DeserializeObject<List<string>>(json)!;
    }


    public string GetWelcomeMessage(IGuildUser user)
        => Messages[Randomizer.Next(Messages.Count - 1)].Replace("[]", user.Mention);
    public IEnumerable<string> GetWelcomeMessages()
        => Messages;

    public void AddWelcomeMessage(string message)
    {
        Messages.Add(message);
        File.WriteAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + "/Data/welcome_messages.json",
                          JsonConvert.SerializeObject(Messages));
    }
}
