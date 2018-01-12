using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Discord.Commands;
using Discord;
using HookAppDiscord.HookApp;

namespace HookAppDiscord.Discord.Modules
{
    public class ServiceModule : ModuleBase<SocketCommandContext>
    {
        [Command("stats")]
        [Summary("Gives you information about a service (services available: hookapp)")]
        public async Task StatusAsync([Remainder] [Summary("Name of service")] string service)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            switch (service)
            {
                case "hookapp":
                    var serverStatus = ApiEndpoint.GetServerStats();
                    builder.Description = "Here are some stats for HookApp";

                    builder.AddField(x =>
                    {
                        x.Name = "Number of users";
                        x.Value = serverStatus.numOfUsers;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Facebook users";
                        x.Value = serverStatus.facebookUsers;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Google users";
                        x.Value = serverStatus.googleUsers;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Number of rates";
                        x.Value = serverStatus.numOfRates;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Number of matches";
                        x.Value = serverStatus.numOfMatches;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Deleted accounts";
                        x.Value = serverStatus.deletedUsers;
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Active users in last 3 days";
                        x.Value = serverStatus.lastActiveUsers;
                        x.IsInline = false;
                    });

                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                    break;
            }
        }
    }
}
