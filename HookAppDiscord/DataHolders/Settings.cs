using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Diagnostics;
using Newtonsoft.Json;

namespace HookAppDiscord.DataHolders
{
    class Settings
    {
        [JsonIgnore]
        public string Error { get; set; }

        public string Token { get; set; } = string.Empty;

        public Settings(string error = "")
        {
            Error = error;
        }

        public static Settings FromJson(string file)
        {
            if (!File.Exists(file))
            {
                File.WriteAllText(file, Generate());
                Process.Start(file);
                return new Settings("File doesn't exist. A new settings file has been written.");
            }

            string json = File.ReadAllText(file);

            try
            {
                return JsonConvert.DeserializeObject<Settings>(json);
            }
            catch (JsonException ex)
            {
                return new Settings($"Unable to parse save file: {ex.Message}");
            }
        }

        private static string Generate()
        {
            return JsonConvert.SerializeObject(new Settings(), Formatting.Indented);
        }
    }
}
