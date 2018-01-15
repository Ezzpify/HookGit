using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;
using Discord;
using Discord.Commands;

namespace HookAppDiscord.Discord.Modules
{
    public class HelpModule : ModuleBase<SocketCommandContext>
    {
        private readonly CommandService _service;
        private readonly ILog _log;

        public HelpModule(CommandService service, ILog log)
        {
            _service = service;
            _log = log;
        }

        [Command("help")]
        [Summary("Lists all available commands")]
        public async Task HelpAsync()
        {
            _log.Info($"{Context.User.Username} executed !help command");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = "These are the commands that you can use"
            };

            foreach (var module in _service.Modules)
            {
                string description = null;
                foreach (var cmd in module.Commands)
                {
                    var result = await cmd.CheckPreconditionsAsync(Context);
                    if (result.IsSuccess)
                        description += $"!{cmd.Aliases.First()}\n";
                }

                if (!string.IsNullOrWhiteSpace(description))
                {
                    builder.AddField(x =>
                    {
                        x.Name = module.Name;
                        x.Value = description;
                        x.IsInline = false;
                    });
                }
            }

            _log.Info("Replying with a list of available commands");
            await ReplyAsync("", false, builder.Build());
        }

        [Command("help")]
        [Summary("Lists information about a specific command")]
        public async Task HelpAsync(string command)
        {
            _log.Info($"{Context.User.Username} executed !help command with parameters {command}");

            var result = _service.Search(Context, command);

            if (!result.IsSuccess)
            {
                await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.");
                return;
            }
            
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"Here are some commands like **{command}**"
            };

            foreach (var match in result.Commands)
            {
                var cmd = match.Command;

                builder.AddField(x =>
                {
                    x.Name = string.Join(", ", cmd.Aliases);
                    x.Value = $"Parameters: {string.Join(", ", cmd.Parameters.Select(p => p.Name))}\n" +
                              $"Summary: {cmd.Summary}";
                    x.IsInline = false;
                });
            }

            _log.Info("Replying with information about matching command(s)");
            await ReplyAsync("", false, builder.Build());
        }
    }
}
