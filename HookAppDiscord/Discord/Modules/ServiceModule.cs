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
                    if (!string.IsNullOrWhiteSpace(serverStatus.error))
                    {
                        builder.Description = $"Encountered an error while getting stats:\n{serverStatus.error}";
                        await Context.Channel.SendMessageAsync("", false, builder.Build());
                        return;
                    }

                    await Context.Channel.SendMessageAsync("", false, DiscordMessageFormatter.GetServiceHookappMessage(serverStatus).Build());
                    break;
            }
        }
    }
}
