using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Discord;
using Discord.Commands;
using log4net;
using Octokit;
using HookAppDiscord.HookApp;

namespace HookAppDiscord.Discord.Modules
{
    public class BasicModule : ModuleBase<SocketCommandContext>
    {
        private readonly ILog _log;
        private List<string> _eightBallAnswers = new List<string>()
        {
            "It is certain",
            "It is decidedly so",
            "Without a doubt",
            "Yes definitely",
            "You may rely on it",
            "As I see it, yes",
            "Most likely",
            "Outlook good",
            "Yes",
            "Signs point to yes",
            "Reply hazy try again",
            "Ask again later",
            "Better not tell you now",
            "Cannot predict now",
            "Concentrate and ask again",
            "Don't count on it",
            "My reply is no",
            "My sources say no",
            "Outlook not so good",
            "Very doubtful"
        };

        public BasicModule(ILog log)
        {
            _log = log;
        }

        [Command("roll")]
        [Summary("Rolls a random number from 1 to n")]
        public async Task RollAsync([Summary("Max roll")] int num)
        {
            _log.Info($"{Context.User.Username} executed !roll command with parameter {num}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{Context.User.Username} rolled {Utils.GetRandom().Next(1, num)}"
            };

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }

        [Command("8ball")]
        [Summary("Gives you a certain answer to your question")]
        public async Task BallAsync([Remainder] [Summary("Question")] string question)
        {
            _log.Info($"{Context.User.Username} executed !8ball command with parameter {question}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{_eightBallAnswers[Utils.GetRandom().Next(0, _eightBallAnswers.Count - 1)]}"
            };

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
