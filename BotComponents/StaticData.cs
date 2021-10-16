using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;

namespace BotComponents
{
    public class StaticData
    {
        public static string Language { get; set; }

        public static Dictionary<string, string> GetLanguageTokens(string language)
        {
            Language = language;

            var json = File.ReadAllText(Directory.GetCurrentDirectory() + $"/Languages/{language}.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}
