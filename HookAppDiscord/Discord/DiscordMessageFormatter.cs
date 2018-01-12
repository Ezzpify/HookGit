using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RestSharp;
using System.Globalization;
using HookAppDiscord.Github.EventHolders;
using HookAppDiscord.HookApp.DataHolders;

namespace HookAppDiscord.Discord
{
    class DiscordMessageFormatter
    {
        public static string GetServerStatsMessage(ServerStats stats)
        {
            return string.Format("Here are the current stats\n```\nNumber of users: {0}\nFacebook users: {1}\nGoogle users: {2}\nNumber of rates: {3}\nNumber of matches: {4}\nDeleted users count: {5}\nActive users in last 3 days: {6}\n```",
                stats.numOfUsers,
                stats.facebookUsers,
                stats.googleUsers,
                stats.numOfRates,
                stats.numOfMatches,
                stats.deletedUsers,
                stats.lastActiveUsers);
        }

        public static string GetRestResponseMessage(IRestResponse code, string endpoint, long responseTime)
        {
            return string.Format("@everyone\n\n**Server ping didn't return OK. Server might be down!**\n\nResponse status:\n```Code: {0}\nDescription: {1}\nResponse time: {2} ms\nEndpoint hit: {3}\n```",
                (int)code.StatusCode,
                code.StatusDescription,
                responseTime,
                endpoint);
        }

        public static string GetRestResponseFailedMessage(string error, string endpoint)
        {
            return string.Format("@everyone\n\n**Server ping failed! Server could be down!**\n\nInformation:\n```\nError: {0}\nEndpoint hit: {1}\n```",
                error,
                endpoint);
        }

        public static string GetOnPushMessage(PushEvent.RootObject obj)
        {
            return string.Format("**{0} pushed {1} commit(s) to '{2}'**\n\n{3}",
                obj.sender.login,
                obj.commits.Count,
                obj.repository.full_name,
                GetCommitsFormatted(obj.commits));
        }

        private static string GetCommitsFormatted(List<PushEvent.Commit> commits)
        {
            var returnList = new List<string>();
            foreach (var commit in commits)
            {
                returnList.Add(string.Format("{0}\n```\n{1}\n\nAdded: {2}\nRemoved: {3}\nModified: {4}\n```",
                    commit.url,
                    commit.message,
                    commit.added.Count,
                    commit.removed.Count,
                    commit.modified.Count));
            }

            return string.Join("\n\n", returnList);
        }

        public static string GetOnCommitCommentMessage(CommitCommentEvent.RootObject obj)
        {
            return string.Format("**{0} {1} a comment on commit '{2}' for '{3}'**\n\n{4}\n```\n{5}\n```",
                obj.comment.user.login,
                obj.action,
                obj.comment.commit_id,
                obj.repository.full_name,
                obj.comment.html_url,
                obj.comment.body);
        }

        public static string GetOnIssuesOpenedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} opened an issue for '{1}'**\n\n{2}\n```\n• {3}\n\n{4}\n\nLabels: {5}\nAssignees: {6}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.title,
                obj.issue.body,
                string.Join(", ", obj.issue.labels.Select(o => o.name)),
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesClosedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} closed issue #{1} for '{2}'**\n\n{3}",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url);
        }

        public static string GetOnIssuesReopenedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} re-opened an issue for '{1}'**\n\n{2}\n```\n• {3}\n\n{4}\n\nLabels: {5}\nAssignees: {6}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.title,
                obj.issue.body,
                string.Join(", ", obj.issue.labels.Select(o => o.name)),
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesAssignedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} assigned a user to issue #{1} for '{2}'**\n\n{3}\n```\n+ {4}\n\nCurrent assignees: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.assignee.login,
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesUnassignedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} un-assigned a user to issue #{1} for '{2}'**\n\n{3}\n```\n- {4}\n\nCurrent assignees: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.assignee.login,
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesLabeledMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} added a label for issue #{1} for '{2}'**\n\n{3}\n```\n+ {4}\n\nCurrent labels: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.label.name,
                string.Join(", ", obj.issue.labels.Select(o => o.name)));
        }

        public static string GetOnIssuesUnlabeledMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} removed a label for issue #{1} for '{2}'**\n\n{3}\n```\n- {4}\n\nCurrent labels: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.label.name,
                string.Join(", ", obj.issue.labels.Select(o => o.name)));
        }

        public static string GetOnIssueCommentMessage(IssueCommentEvent.RootObject obj)
        {
            return string.Format("**{0} {1} a comment on an issue #{2}**\n\n{3}\n```\n{4}\n\nTitle: {5}\nAssignees: {6}\nLabels: {7}\n```",
                obj.comment.user.login,
                obj.action,
                obj.issue.number,
                obj.comment.html_url,
                obj.comment.body,
                obj.issue.title,
                string.Join(", ", obj.issue.assignees.Select(o => o.login)),
                string.Join(", ", obj.issue.labels.Select(o => o.name)));
        }

        public static string GetOnLabelCreatedMessage(LabelEvent.RootObject obj)
        {
            return string.Format("**{0} created a new label for '{1}'**\n```\n+ {2}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.label.name);
        }

        public static string GetOnLabelEditedMessage(LabelEvent.RootObject obj)
        {
            return string.Format("**{0} edited a label for '{1}'**\n```\n{2} -> {3}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.changes.name.from,
                obj.label.name);
        }

        public static string GetOnLabelDeletedMessage(LabelEvent.RootObject obj)
        {
            return string.Format("**{0} deleted a label for '{1}'**\n```\n- {2}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.label.name);
        }

        public static string GetOnProjectCardCreatedMessage(ProjectCardEvent.RootObject obj)
        {
            return string.Format("**{0} created a new project card for '{1}'**\n```\n{2}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.project_card.note);
        }

        public static string GetOnProjectCardEditedMessage(ProjectCardEvent.RootObject obj)
        {
            return string.Format("**{0} edited a project card for '{1}'**\n```\n{2}\n----\nOriginal: {3}\nCreated at: {4}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.project_card.note,
                obj.changes.note.from,
                obj.project_card.created_at.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static string GetOnProjectCardMovedMessage(ProjectCardEvent.RootObject obj)
        {
            return string.Format("**{0} moved a project card for '{1}'**\n```\n{2}\n----\nCreated at: {3}\n```",
                obj.sender.login,
                obj.repository.full_name,
                obj.project_card.note,
                obj.project_card.created_at.ToString("MM/dd/yyyy HH:mm:ss", CultureInfo.InvariantCulture));
        }

        public static string GetOnProjectCardConvertedMessage(ProjectCardEvent.RootObject obj)
        {
            return string.Format("**{0} converted a project card for '{1}' into an issue**\n{2}",
                obj.sender.login,
                obj.repository.full_name,
                obj.project_card.content_url.Replace("api.", "").Replace("repos/", ""));
        }

        public static string GetOnProjectCardDeletedMessage(ProjectCardEvent.RootObject obj)
        {
            return string.Format("**{0} deleted a project card for '{1}'**",
                obj.sender.login,
                obj.repository.full_name);
        }
    }
}
