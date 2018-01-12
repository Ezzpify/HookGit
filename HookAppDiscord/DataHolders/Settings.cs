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

        public string DiscordToken { get; set; } = string.Empty;

        public string AzureToken { get; set; } = string.Empty;

        public string CleverbotToken { get; set; } = string.Empty;

        public string GithubToken { get; set; } = string.Empty;

        public string ServerEndpointStatus { get; set; } = "http://0.0.0.0:9000/json/endpoint";

        public string ServerEndpointStats { get; set; } = "http://192.168.0.38:9100/json/endpoint";

        public string TranslateTo { get; set; } = "en";

        public ulong TranslationChannel { get; set; } = 0;

        public ulong GithubChannel { get; set; } = 0;

        public ulong StatusChannel { get; set; } = 0;

        public double StatusPingInterval { get; set; } = 15.00;

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
