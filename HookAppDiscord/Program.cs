using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord
{
    class Program
    {
        private static Session _session;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Console.Title = "HookGit | Discord bot";
            var settings = Settings.FromJson(Const.SETTINGS_FILE_PATH);
            if (!String.IsNullOrEmpty(settings.Error))
            {
                Console.WriteLine(settings.Error);
            }
            else
            {
                _session = new Session(settings);
                await _session.Connect();
                await Task.Delay(-1);
            }
        }
    }
}
