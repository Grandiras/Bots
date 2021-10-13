﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace BotComponents
{
    public class StaticData
    {
        public static Dictionary<string, string> GetLanguageTokens(string language)
        {
            var json = File.ReadAllText("C:/Users/Micha/Desktop/Darkymos Projects/Github/Scripts/Bots/BotComponents" + $"/Languages/{language}.json");
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(json);
        }
    }
}
