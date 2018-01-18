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
using log4net;
using Octokit;
using Octokit.Reactive;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord.Discord.Modules
{
    public class GithubModule : ModuleBase<SocketCommandContext>
    {
        private readonly GitHubClient _client;
        private readonly Settings _settings;
        private readonly ILog _log;

        public GithubModule(GitHubClient client, Settings settings, ILog log)
        {
            _client = client;
            _settings = settings;
            _log = log;
        }

        [Command("createissue")]
        [Summary("Create a new GitHub issue")]
        public async Task CreateIssueAsync([Summary("Owner of repository")] string owner, [Summary("Name of repository")] string repo, [Summary("Title of the issue")] string title, [Summary("Body of the issue")] string body)
        {
            _log.Info($"{Context.User.Username} executed !createissue command with parameters {repo} | {title} | {body}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            var issue = new NewIssue(title);
            issue.Body = body;
            
            var newIssue = await _client.Issue.Create(owner, repo, issue);

            if (newIssue != null)
            {
                builder.Description = $"Issue created for '{repo}' with issue number #{newIssue.Number}";
                builder.AddField(x =>
                {
                    x.Name = "Url";
                    x.Value = newIssue.HtmlUrl;
                    x.IsInline = false;
                });

                _log.Info($"Issue #{newIssue.Number} has been created for repo '{repo}'");
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                _log.Error($"Unable to create issue. Issue returned null.");
            }
        }

        [Command("closeissue")]
        [Summary("Closes a GitHub issue")]
        public async Task CloseIssueAsync([Summary("Owner of repository")] string owner, [Summary("Name of repository")] string repo, [Summary("Issue number")] int number)
        {
            _log.Info($"{Context.User.Username} executed !closeissue command with parameters {repo} | {number}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };
            
            var update = new IssueUpdate();
            update.State = ItemState.Closed;

            var newIssue = await _client.Issue.Update(owner, repo, number, update);
            if (newIssue != null)
            {
                builder.Description = $"Issue #{newIssue.Number} has been closed.";
                builder.AddField(x =>
                {
                    x.Name = "Url";
                    x.Value = newIssue.HtmlUrl;
                    x.IsInline = false;
                });

                _log.Info($"Issue #{number} has been closed.");
                await Context.Channel.SendMessageAsync("", false, builder.Build());
            }
            else
            {
                _log.Error($"Unable to close issue. Issue returned null.");
            }
        }

        [Command("assignissue")]
        [Summary("Assigns user to issue")]
        public async Task AssignIssueAsync([Summary("Owner of repository")] string owner, [Summary("Name of repository")] string repo, [Summary("Issue number")] int number, SocketUser user = null)
        {
            _log.Info($"{Context.User.Username} executed !assignissue command with parameters {repo} | {number}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            string githubUser = string.Empty;
            if (user == null || string.IsNullOrEmpty(githubUser = _settings.Users.FirstOrDefault(o => o.DiscordId == user.Id).GithubUsername))
            {
                _log.Error($"User provided does not exist in my list of users.");
                builder.Description = "I don't seem to have that user in my list.";
                await Context.Channel.SendMessageAsync("", false, builder.Build());
                return;
            }

            _log.Info($"Targeted GitHub user is: {githubUser}");

            var issue = await _client.Issue.Get(owner, repo, number);
            if (issue != null)
            {
                var update = issue.ToUpdate();
                update.AddAssignee(githubUser);

                var newIssue = await _client.Issue.Update(owner, repo, number, update);
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

                    builder.AddField(x =>
                    {
                        x.Name = "Url";
                        x.Value = newIssue.HtmlUrl;
                        x.IsInline = false;
                    });

                    _log.Info($"User {githubUser} has been assigned to issue #{number}");
                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    _log.Error($"Unable to assign user to issue. Issue returned null.");
                }
            }
        }

        [Command("labelissue")]
        [Summary("Assigns label to issue")]
        public async Task LabelIssueAsync([Summary("Owner of repository")] string owner, [Summary("Name of repository")] string repo, [Summary("Issue number")] int number, [Summary("Name of label")] string label)
        {
            _log.Info($"{Context.User.Username} executed !labelissue command with parameters {repo} | {number} | {label}");

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
            };

            if (!_settings.Labels.Contains(label))
            {
                _log.Error($"Label {label} does not exist. Replying with list of available labels.");
                builder.Description = "Label does not exist. List of available labels:";
                foreach (var labelName in _settings.Labels)
                {
                    builder.AddField(x =>
                    {
                        x.Name = "Label name";
                        x.Value = labelName;
                        x.IsInline = false;
                    });
                }
                
                await Context.Channel.SendMessageAsync("", false, builder.Build());
                return;
            }

            var issue = await _client.Issue.Get(owner, repo, number);
            if (issue != null)
            {
                var update = issue.ToUpdate();
                update.AddLabel(label);

                var newIssue = await _client.Issue.Update(owner, repo, number, update);
                if (newIssue != null)
                {
                    builder.Description = $"Label {label} has been assigned to issue #{newIssue.Number}";

                    builder.AddField(x =>
                    {
                        x.Name = "Current labels";
                        x.Value = string.Join(", ", newIssue.Labels.Select(o => o.Name));
                        x.IsInline = false;
                    });

                    builder.AddField(x =>
                    {
                        x.Name = "Url";
                        x.Value = newIssue.HtmlUrl;
                        x.IsInline = false;
                    });

                    _log.Info($"Assigned label {label} to issue #{number}");
                    await Context.Channel.SendMessageAsync("", false, builder.Build());
                }
                else
                {
                    _log.Error($"Unable to label issue. Issue returned null.");
                }
            }
            else
            {
                _log.Error("Unable to label issue. Could not get issue.");
            }
        }

        [Command("listissues")]
        [Summary("Lists all issues for repo")]
        public async Task ListIssueAsync([Summary("Owner of repository")] string owner, [Summary("Name of repository")] string repo)
        {
            _log.Info($"{Context.User.Username} executed !listissues command with parameter {repo}");
            
            var issueList = await _client.Issue.GetAllForRepository(owner, repo);

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"All issues for {repo}"
            };

            foreach (var issue in issueList)
            {
                string assignString = string.Join("\n• ", issue.Assignees.Select(o => o.Login));
                assignString = !string.IsNullOrEmpty(assignString) ? $"Assigned:\n• {assignString}" : "Assigned:\nNone";

                builder.AddField(x =>
                {
                    x.Name = $"#{issue.Number} - {issue.Title}";
                    x.Value = $"{issue.HtmlUrl}\n{Utils.Truncate(issue.Body, 100)}\n{assignString}";
                    x.IsInline = false;
                });
            }

            _log.Info($"Replying with {issueList.Count} issues");
            await Context.Channel.SendMessageAsync("", false, builder.Build());
        }
    }
}
