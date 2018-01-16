using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using Discord;
using System.Globalization;
using HookAppDiscord.Github.EventHolders;
using HookAppDiscord.HookApp.DataHolders;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord.Discord
{
    class DiscordMessageFormatter
    {
        public static EmbedBuilder GetTranslationMessage(Translation translation)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"Translated {translation.Author}'s message in #{translation.Channel}"
            };

            builder.AddField(x =>
            {
                x.Name = "Translated text";
                x.Value = translation.TranslatedText;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Original text";
                x.Value = translation.OriginalText;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetServiceHookappMessage(ServerStats stats, ServerStats oldStats)
        {
            if (oldStats == null)
            {
                oldStats = new ServerStats();
                oldStats.date = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            }

            var ts = stats.date.Subtract(oldStats.date);

            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"Here are some stats for HookApp with data difference from {ts.Days} days, {ts.Hours} hours, {ts.Minutes} minutes and {ts.Seconds} seconds ago."
            };

            builder.AddField(x =>
            {
                x.Name = "Number of users";
                x.Value = $"{oldStats.numOfUsers} -> {stats.numOfUsers} [{Utils.GetIncreasement(stats.numOfUsers, oldStats.numOfUsers)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Facebook users";
                x.Value = $"{oldStats.facebookUsers} -> {stats.facebookUsers} [{Utils.GetIncreasement(stats.facebookUsers, oldStats.facebookUsers)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Google users";
                x.Value = $"{oldStats.googleUsers} -> {stats.googleUsers} [{Utils.GetIncreasement(stats.googleUsers, oldStats.googleUsers)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Number of rates";
                x.Value = $"{oldStats.numOfRates} -> {stats.numOfRates} [{Utils.GetIncreasement(stats.numOfRates, oldStats.numOfRates)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Number of matches";
                x.Value = $"{oldStats.numOfMatches} -> {stats.numOfMatches} [{Utils.GetIncreasement(stats.numOfMatches, oldStats.numOfMatches)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Deleted accounts";
                x.Value = $"{oldStats.deletedUsers} -> {stats.deletedUsers} [{Utils.GetIncreasement(stats.deletedUsers, oldStats.deletedUsers)}]";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Active users in last 3 days";
                x.Value = $"{oldStats.lastActiveUsers} -> {stats.lastActiveUsers} [{Utils.GetIncreasement(stats.lastActiveUsers, oldStats.lastActiveUsers)}]";
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetRestResponseMessage(IRestResponse code, string endpoint, long responseTime)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = "Server ping didn't return OK. Server might be down!"
            };

            builder.AddField(x =>
            {
                x.Name = "Status code";
                x.Value = (int)code.StatusCode;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Response status";
                x.Value = string.IsNullOrWhiteSpace(code.StatusDescription) ? "Description not available" : code.StatusDescription;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Response time";
                x.Value = responseTime;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Endpoint hit";
                x.Value = endpoint;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetRestResponseFailedMessage(string error, string endpoint)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = "Server ping failed! Server could be down!"
            };

            builder.AddField(x =>
            {
                x.Name = "Error";
                x.Value = error;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Endpoint hit";
                x.Value = endpoint;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnPushMessage(PushEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} pushed {obj.commits.Count} commit(s) to {obj.repository.full_name}"
            };

            foreach (var commit in obj.commits)
            {
                builder.AddField(x =>
                {
                    x.Name = commit.message;
                    x.Value = $"{commit.url}\nAdded: {commit.added.Count}\nRemoved: {commit.removed.Count}\nModified: {commit.modified.Count}";
                    x.IsInline = false;
                });
            }

            return builder;
        }

        public static EmbedBuilder GetOnCommitCommentMessage(CommitCommentEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} {obj.action} a comment on commit {obj.comment.commit_id} for {obj.repository.full_name}"
            };
            
            builder.AddField(x =>
            {
                x.Name = "Comment";
                x.Value = obj.comment.body;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.comment.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesOpenedMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} opened issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = obj.issue.title;
                x.Value = obj.issue.body;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Assignees";
                x.Value = obj.issue.assignees.Count > 0 ? string.Join(", ", obj.issue.assignees.Select(o => o.login)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Labels";
                x.Value = obj.issue.labels.Count > 0 ? string.Join(", ", obj.issue.labels.Select(o => o.name)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesClosedMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} closed issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = obj.issue.title;
                x.Value = obj.issue.body;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesReopenedMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} re-opened issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = obj.issue.title;
                x.Value = obj.issue.body;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Assignees";
                x.Value = obj.issue.assignees.Count > 0 ? string.Join(", ", obj.issue.assignees.Select(o => o.login)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Labels";
                x.Value = obj.issue.labels.Count > 0 ? string.Join(", ", obj.issue.labels.Select(o => o.name)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesAssignedMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} assigned user to issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Assigned user";
                x.Value = obj.assignee.login;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Users assigned to issue";
                x.Value = obj.issue.assignees.Count > 0 ? string.Join(", ", obj.issue.assignees.Select(o => o.login)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesUnassignedMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} un-assigned user to issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Unassigned user";
                x.Value = obj.assignee.login;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Users assigned to issue";
                x.Value = obj.issue.assignees.Count > 0 ? string.Join(", ", obj.issue.assignees.Select(o => o.login)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesLabeledMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} added a label to issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Added label";
                x.Value = obj.label.name;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Current labels for issue";
                x.Value = obj.issue.labels.Count > 0 ? string.Join(", ", obj.issue.labels.Select(o => o.name)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssuesUnlabeledMessage(IssuesEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} removed a label from issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Removed label";
                x.Value = obj.label.name;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Current labels for issue";
                x.Value = obj.issue.labels.Count > 0 ? string.Join(", ", obj.issue.labels.Select(o => o.name)) : "None";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnIssueCommentMessage(IssueCommentEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} {obj.action} a comment on issue #{obj.issue.number} for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = $"Comment {obj.action}";
                x.Value = obj.comment.body;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.issue.html_url;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnLabelCreatedMessage(LabelEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} created a new label for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "New label";
                x.Value = obj.label.name;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Label color";
                x.Value = $"#{obj.label.color}";
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnLabelEditedMessage(LabelEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} edited a label for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "New name";
                x.Value = obj.label.name;
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Original";
                x.Value = obj.changes.name.from;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnLabelDeletedMessage(LabelEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} deleted a label for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Deleted label";
                x.Value = obj.label.name;
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnProjectCardCreatedMessage(ProjectCardEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} created a new project card for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Card note";
                x.Value = !string.IsNullOrEmpty(obj.project_card.note) ? obj.project_card.note : "Not available";
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnProjectCardEditedMessage(ProjectCardEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} edited a project card for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "New note";
                x.Value = !string.IsNullOrEmpty(obj.project_card.note) ? obj.project_card.note : "Not available";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Original";
                x.Value = !string.IsNullOrEmpty(obj.changes.note.from) ? obj.changes.note.from : "Not available";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Creation date";
                x.Value = obj.project_card.created_at.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnProjectCardMovedMessage(ProjectCardEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} moved a project card for {obj.repository.full_name}"
            };

            builder.AddField(x =>
            {
                x.Name = "Card note";
                x.Value = !string.IsNullOrEmpty(obj.project_card.note) ? obj.project_card.note : "Not available";
                x.IsInline = false;
            });

            builder.AddField(x =>
            {
                x.Name = "Creation date";
                x.Value = obj.project_card.created_at.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnProjectCardConvertedMessage(ProjectCardEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} converted a project card for {obj.repository.full_name} into a new issue"
            };

            builder.AddField(x =>
            {
                x.Name = "Url";
                x.Value = obj.project_card.content_url.Replace("api.", "").Replace("repos/", "");
                x.IsInline = false;
            });

            return builder;
        }

        public static EmbedBuilder GetOnProjectCardDeletedMessage(ProjectCardEvent.RootObject obj)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = $"{obj.sender.login} deleted a project card from {obj.repository.full_name}"
            };

            return builder;
        }
    }
}