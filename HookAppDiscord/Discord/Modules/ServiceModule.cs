using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Newtonsoft.Json;
using log4net;
using Discord.Commands;
using Discord;
using HookAppDiscord.HookApp;
using HookAppDiscord.HookApp.DataHolders;

namespace HookAppDiscord.Discord.Modules
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILog _log;
        private readonly List<ServerStats> _stats;

        public ServiceModule(ILog log, List<ServerStats> stats)
        {
            _log = log;
            _stats = stats;
        }

        [Command("stats")]
        [Summary("Gives you information about a service (services available: hookapp)")]
        public async Task StatusAsync([Summary("Name of service")] string service, [Summary("How many hours to go back for comparison stats")]  int hours = 0)
        {
            _log.Info($"{Context.User.Username} executed !stats command with parameter {service}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            switch (service)
            {
                case "hookapp":
                    ServerStats stats = _stats.Where(o => string.IsNullOrEmpty(o.error)).LastOrDefault();
                    ServerStats compared = null;

                    if (stats == null)
                    {
                        _log.Info($"Ho history of stats available. Forcefully getting one from server.");
                        stats = ApiEndpoint.GetServerStats();
                    }

                    var historyList = _stats.Where(o => string.IsNullOrEmpty(o.error)).Take(_stats.Count() - 1).ToList();
                    if (hours.Equals(0))
                    {
                        compared = historyList.LastOrDefault();
                    }
                    else
                    {
                        _log.Info($"Getting history stats from {hours} hours ago");
                        var queryDay = DateTime.Now.Subtract(TimeSpan.FromHours(hours));
                        compared = historyList.OrderBy(o => Math.Abs((o.date.Subtract(queryDay).Ticks))).FirstOrDefault();
                    }

                    _log.Info($"Replying with server status for service {service}");
                    await Context.Channel.SendMessageAsync("", false, DiscordMessageFormatter.GetServiceHookappMessage(stats, compared).Build());
                    break;
            }
        }
    }
}