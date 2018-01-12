using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using HookAppDiscord.HookApp;

namespace HookAppDiscord.Discord.Modules
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        [Command("stats")]
        [Summary("Gives you information about a service (services available: hookapp)")]
        public async Task StatusAsync([Remainder] [Summary("Name of service")] string service)
        {
            switch (service)
            {
                case "hookapp":
                    var serverStatus = ApiEndpoint.GetServerStats();
                    await Context.Channel.SendMessageAsync(DiscordMessageFormatter.GetServerStatsMessage(serverStatus));
                    break;
            }
        }
    }
}
