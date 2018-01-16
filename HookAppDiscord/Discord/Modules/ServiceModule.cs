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

        public ServiceModule(ILog log)
        {
            _log = log;
        }

        [Command("stats")]
        [Summary("Gives you information about a service (services available: hookapp)")]
        public async Task StatusAsync([Summary("Name of service")] string service, [Summary("How many days to go back for comparison stats")] int days = 0)
        {
            _log.Info($"{Context.User.Username} executed !stats command with parameter {service}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            switch (service)
            {
                case "hookapp":
                    var serverStats = ApiEndpoint.GetServerStats();
                    if (!string.IsNullOrWhiteSpace(serverStats.error))
                    {
                        _log.Error($"Unable to get server stats for {service}. Error: {serverStats.error}");
                        builder.Description = $"Encountered an error while getting stats:\n{serverStats.error}";
                        await Context.Channel.SendMessageAsync("", false, builder.Build());
                        return;
                    }

                    var statsList = new List<ServerStats>();

                    if (File.Exists(Const.SERVICE_HOOKAPP_HISTORY))
                    {
                        string historyJson = File.ReadAllText(Const.SERVICE_HOOKAPP_HISTORY);
                        statsList.AddRange(JsonConvert.DeserializeObject<List<ServerStats>>(historyJson));
                    }

                    ServerStats compared = null;
                    if (days == 0)
                    {
                        compared = statsList.LastOrDefault();
                    }
                    else
                    {
                        _log.Info($"Getting history stats from {days} days ago");
                        var queryDay = DateTime.Now.Subtract(TimeSpan.FromDays(days));
                        compared = statsList.OrderBy(o => Math.Abs((o.date.Subtract(queryDay).Ticks))).FirstOrDefault();
                    }
                    
                    statsList.Add(serverStats);
                    File.WriteAllText(Const.SERVICE_HOOKAPP_HISTORY, JsonConvert.SerializeObject(statsList, Formatting.Indented));

                    _log.Info($"Replying with server status for service {service}");
                    await Context.Channel.SendMessageAsync("", false, DiscordMessageFormatter.GetServiceHookappMessage(serverStats, compared).Build());
                    break;
            }
        }
    }
}
