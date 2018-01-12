using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Discord;
using Discord.Commands;
using HookAppDiscord.HookApp;
using Octokit;
using Octokit.Reactive;

namespace HookAppDiscord.Discord.Modules
{
    public class GithubModule : ModuleBase<SocketCommandContext>
    {
        private readonly GitHubClient _client;
        private readonly string _repoOwner = "Ezzpify";

        public GithubModule(GitHubClient client)
        {
            _client = client;
        }

        [Command("createissue")]
        [Summary("Create a new GitHub issue")]
        public async Task CreateIssueAsync([Summary("Name of repository")] string repo, [Summary("Title of the issue")] string title, [Summary("Body of the issue")] string body)
        {
            var issue = new NewIssue(title);
            issue.Body = body;

            await _client.Issue.Create(_repoOwner, repo, issue);
        }

        [Command("closeissue")]
        [Summary("Closes a GitHub issue")]
        public async Task CloseIssueAsync([Summary("Name of repository")] string repo, [Summary("Issue number")] int number)
        {
            await _client.Issue.Lock(_repoOwner, repo, number);
        }
    }
}
