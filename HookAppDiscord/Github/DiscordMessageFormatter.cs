using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HookAppDiscord.Github.EventHolders;

namespace HookAppDiscord.Github
{
    class DiscordMessageFormatter
    {
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
            return string.Format("**{0} assigned a user to issue #{1} for '{2}'**\n\n{3}\n```\n+{4}\n\nCurrent assignees: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.assignee.login,
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesUnassignedMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} un-assigned a user to issue #{1} for '{2}'**\n\n{3}\n```\n-{4}\n\nCurrent assignees: {5}\n```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.issue.assignee.login,
                string.Join(", ", obj.issue.assignees.Select(o => o.login)));
        }

        public static string GetOnIssuesLabeledMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} added a label for issue #{1} for '{2}'**\n\n{3}\n```\n+{4}\n\nCurrent labels: {5}```",
                obj.sender.login,
                obj.issue.number,
                obj.repository.full_name,
                obj.issue.html_url,
                obj.label.name,
                string.Join(", ", obj.issue.labels.Select(o => o.name)));
        }

        public static string GetOnIssuesUnlabeledMessage(IssuesEvent.RootObject obj)
        {
            return string.Format("**{0} removed a label for issue #{1} for '{2}'**\n\n{3}\n```\n-{4}\n\nCurrent labels: {5}```",
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
    }
}
