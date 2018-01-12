using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using HookAppDiscord.HookApp;
using Octokit;
using Octokit.Reactive;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord.Discord.Modules
{
    public class GithubModule : ModuleBase<SocketCommandContext>
    {
        private readonly GitHubClient _client;
        private readonly Settings _settings;

        public GithubModule(GitHubClient client, Settings settings)
        {
            _client = client;
            _settings = settings;
        }

        [Command("createissue")]
        [Summary("Create a new GitHub issue")]
        public async Task CreateIssueAsync([Summary("Name of repository")] string repo, [Summary("Title of the issue")] string title, [Summary("Body of the issue")] string body)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            var issue = new NewIssue(title);
            issue.Body = body;

            var owner = await _client.User.Current();
            var newIssue = await _client.Issue.Create(owner.Login, repo, issue);

            if (newIssue != null)
            {
                builder.Description = $"Issue created for '{repo}' with issue number #{newIssue.Number}";
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("closeissue")]
        [Summary("Closes a GitHub issue")]
        public async Task CloseIssueAsync([Summary("Name of repository")] string repo, [Summary("Issue number")] int number)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };
            
            var update = new IssueUpdate();
            update.State = ItemState.Closed;

            var owner = await _client.User.Current();
            var newIssue = await _client.Issue.Update(owner.Login, repo, number, update);
            if (newIssue != null)
            {
                builder.Description = $"Issue #{newIssue.Number} has been closed.";
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("assignissue")]
        [Summary("Assigns user to issue")]
        public async Task AssignIssueAsync([Summary("Name of repository")] string repo, [Summary("Issue number")] int number, SocketUser user = null)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            string githubUser = string.Empty;
            if (user == null || (githubUser = _settings.Users.FirstOrDefault(o => o.DiscordId == user.Id).GithubUsername) == null)
            {
                builder.Description = "I don't seem to have that user in my list.";
                await Context.Channel.SendMessageAsync("", false, builder.Build());
                return;
            }

            var update = new IssueUpdate();
            update.AddAssignee(githubUser);

            var owner = await _client.User.Current();
            var newIssue = await _client.Issue.Update(owner.Login, repo, number, update);
            if (newIssue != null)
            {
                builder.Description = $"{githubUser} has been assigned to issue #{newIssue.Number}\nCurrent assigneed:";
                foreach (var assignee in newIssue.Assignees)
                {
                    builder.AddField(x =>
                    {
                        x.Name = assignee.Login;
                        x.Value = assignee.HtmlUrl;
                        x.IsInline = false;
                    });
                }

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("labelissue")]
        [Summary("Assigns label to issue")]
        public async Task LabelIssueAsync([Summary("Name of repository")] string repo, [Summary("Issue number")] int number, [Summary("Name of label")] string label)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            if (!_settings.Labels.Contains(label))
            {
                builder.Description = "Label does not exist. List of available labels:";
                foreach (var labelName in _settings.Labels)
                {
                    builder.AddField(x =>
                    {
                        x.Name = labelName;
                        x.IsInline = false;
                    });
                }
                
                await Context.Channel.SendMessageAsync("", false, builder.Build());
                return;
            }

            var update = new IssueUpdate();
            update.AddLabel(label);

            var owner = await _client.User.Current();
            var newIssue = await _client.Issue.Update(owner.Login, repo, number, update);
            if (newIssue != null)
            {
                builder.Description = $"Label {label} has been assigned to issue #{newIssue.Number}\nCurrent labels:";
                foreach (var labelName in newIssue.Labels.Select(o => o.Name))
                {
                    builder.AddField(x =>
                    {
                        x.Name = labelName;
                        x.IsInline = false;
                    });
                }

                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
        }

        [Command("listissues")]
        [Summary("Lists all issues for repo")]
        public async Task LabelIssueAsync([Summary("Name of repository")] string repo)
        {
            var owner = await _client.User.Current();
            var issueList = await _client.Issue.GetAllForRepository(owner.Login, repo);

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"All issues for {repo}"
            };

            foreach (var issue in issueList)
            {
                builder.AddField(x =>
                {
                    x.Name = $"#{issue.Number} - {issue.Title}";
                    x.Value = Utils.Truncate(issue.Body, 50);
                    x.IsInline = false;
                });
            }

            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
