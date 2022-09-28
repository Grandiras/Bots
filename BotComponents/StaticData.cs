using Newtonsoft.Json;

namespace BotComponents;

public sealed class StaticData
{
    public static string? Language { get; set; }

    public static Dictionary<string, string> GetLanguageTokens(string language)
    {
        Language = language;

        var json = File.ReadAllText(Directory.GetCurrentDirectory().Split(@"\bin")[0] + $"/Languages/{language}.json");
        return JsonConvert.DeserializeObject<Dictionary<string, string>>(json)!;
    }
}
