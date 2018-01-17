using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;
using System.Diagnostics;
using System.Net;
using Discord;
using Discord.WebSocket;
using Discord.Commands;
using RestSharp;
using log4net;
using Octokit;
using Octokit.Internal;
using Octokit.Reactive;
using Newtonsoft.Json;
using Cleverbot.Net;
using Microsoft.Extensions.DependencyInjection;
using HookAppDiscord.DataHolders;
using HookAppDiscord.Microsoft;
using HookAppDiscord.WCF;
using HookAppDiscord.Github;
using HookAppDiscord.HookApp;
using HookAppDiscord.HookApp.DataHolders;
using HookAppDiscord.Discord;
using HookAppDiscord.Github.EventHolders;

namespace HookAppDiscord
{
    class Session
    {
        private readonly ILog _log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private Settings _settings;

        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private Translate _translate;
        private ISocketMessageChannel _translationChannel;

        private WCFServer _wcfServer;
        private ISocketMessageChannel _githubChannel;
        private GitHubClient _githubClient;

        private Timer _statusTimer;
        private ISocketMessageChannel _statusChannel;
        private int _statusResponseErrors;

        private CleverbotSession _cleverbot;

        private Timer _serviceHookappTimer;
        private List<ServerStats> _serviceHookappStats;

        public Session(Settings settings)
        {
            _settings = settings;
            
            Console.WriteLine("Setting up endpoint urls...");
            ApiEndpoint.ServerStatsUrl = _settings.ServerEndpointStats;

            Console.WriteLine("Setting up translation...");
            _translate = new Translate(_log, settings.AzureToken, settings.TranslateTo);

            Console.WriteLine("Setting up github webhook...");
            GithubWebhookDelivery callback = GithubDelivery;
            _wcfServer = new WCFServer(callback);
            _wcfServer.Start();

            Console.WriteLine("Getting stats history...");
            if (File.Exists(Const.SERVICE_HOOKAPP_HISTORY))
                _serviceHookappStats = JsonConvert.DeserializeObject<List<ServerStats>>(File.ReadAllText(Const.SERVICE_HOOKAPP_HISTORY));
            else
                _serviceHookappStats = new List<ServerStats>();

            Console.WriteLine("Setting up github access...");
            var credentials = new InMemoryCredentialStore(new Credentials(_settings.GithubToken));
            _githubClient = new GitHubClient(new ProductHeaderValue("HookApp"));
            _githubClient.Credentials = new Credentials(_settings.GithubToken);

            Console.WriteLine("Setting up cleverbot...");
            _cleverbot = new CleverbotSession(_settings.CleverbotToken);

            Console.WriteLine("Setting up discord...");
            _client = new DiscordSocketClient();
            _client.Log += _client_Log;
            _client.Ready += _client_Ready;
            _client.MessageReceived += _client_MessageReceived;
            InstallDiscordCommands();
        }

        private void InstallDiscordCommands()
        {
            _commands = new CommandService();
            _services = new ServiceCollection()
                .AddSingleton(_client)
                .AddSingleton(_commands)
                .AddSingleton(_githubClient)
                .AddSingleton(_settings)
                .AddSingleton(_log)
                .AddSingleton(_serviceHookappStats)
                .BuildServiceProvider();
            
            _commands.AddModulesAsync(Assembly.GetEntryAssembly());
        }

        private async Task HandleCommandAsync(SocketMessage messageParam)
        {
            var message = (SocketUserMessage)messageParam;
            if (message == null)
                return;

            int argPos = 0;
            if (!(message.HasCharPrefix('!', ref argPos) || message.HasMentionPrefix(_client.CurrentUser, ref argPos)))
                return;
            
            var context = new SocketCommandContext(_client, message);

            _log.Info($"Executing command: {message.Content}");
            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
            {
                _log.Error($"Error executing command. Reason: {result.ErrorReason}");
                await context.Channel.SendMessageAsync(result.ErrorReason);
            }
        }

        public async Task Connect()
        {
            await _client.LoginAsync(TokenType.Bot, _settings.DiscordToken);
            await _client.StartAsync();
        }

        private Task _client_Ready()
        {
            String ascii =
                  "  _   _             _    _____ _ _\n"
                + " | | | |           | |  |  __ (_) |\n"
                + " | |_| | ___   ___ | | _| |  \\/_| |_\n"
                + " |  _  |/ _ \\ / _ \\| |/ / | __| | __|\n"
                + " | | | | (_) | (_) |   <| |_\\ \\ | |_\n"
                + " \\_| |_/\\___ /\\___/|_|\\_\\_____/_|\\__|\n\n"
                + $" Author: Casper BL\n"
                + $" Version: {Utils.GetVersion()}\n"
                + $" Build date: {File.GetLastWriteTime(Assembly.GetExecutingAssembly().Location)}\n"
                + $" Discord utility for keeping track of GitHub projects\n"
                + " ----------------------------------------------------------------\n\n";

            _client.SetGameAsync("HookApp");
            _translationChannel = (ISocketMessageChannel)_client.GetChannel(_settings.TranslationChannel);
            _githubChannel = (ISocketMessageChannel)_client.GetChannel(_settings.GithubChannel);
            _statusChannel = (ISocketMessageChannel)_client.GetChannel(_settings.StatusChannel);

            _statusTimer = new Timer(ServerStatusTimer, null, 0, (int)TimeSpan.FromMinutes(_settings.StatusPingInterval).TotalMilliseconds);

            DateTime dueTime = new DateTime(DateTime.Today.Year, DateTime.Today.Month, DateTime.Today.Day, (DateTime.Now.Hour + 1), 0, 0);
            TimeSpan timeRemaining = dueTime.Subtract(DateTime.Now);
            _serviceHookappTimer = new Timer(ServieHookappTimer, null, (int)timeRemaining.TotalMilliseconds, (int)TimeSpan.FromHours(1).TotalMilliseconds);

            Console.Clear();
            Console.WriteLine(ascii);
            return Task.CompletedTask;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                string friendlyMessage = GetFriendlyDiscordMessage(arg);
                _log.Info($"[Discord message] [#{arg.Channel.Name}] {arg.Author.Username}: {arg.Content}");
                
                var message = _translate.GetTranslatedMessage(arg, friendlyMessage);
                if (message != null)
                {
                    await _translationChannel.SendMessageAsync("", false, message);
                }

                if (arg.MentionedUsers.Select(o => o.Id).Contains(_client.CurrentUser.Id))
                {
                    _log.Info($"Getting cleverbot response for user {arg.Author.Username}");

                    CleverbotResponse response = await _cleverbot.GetResponseAsync(friendlyMessage);
                    await arg.Channel.SendMessageAsync(response.Response);
                    _log.Info($"Cleverbot responds: {response.Response}");
                }
                else
                {
                    await HandleCommandAsync(arg);
                }
            }
        }

        private string GetFriendlyDiscordMessage(SocketMessage arg)
        {
            var userDic = new Dictionary<string, string>();
            foreach (var user in arg.MentionedUsers)
            {
                string key = user.Id.ToString();
                if (!userDic.ContainsKey(key))
                    userDic.Add(key, user.Username);
            }

            /*Replace each Discord user id with their username*/
            string converted = Regex.Replace(arg.Content, @"<@!?(\d+)>", 
                m => userDic.ContainsKey(m.Groups[1].Value) 
                ? userDic[m.Groups[1].Value] : m.Value);

            /*Remove the first tagged username if it is the bot's*/
            converted = Regex.Replace(converted, $@"^{_client.CurrentUser.Username}\s*", String.Empty, RegexOptions.Multiline);

            /*Replace multiple spaces with single space*/
            converted = Regex.Replace(converted, @"\s+", " ");

            return converted;
        }

        private async Task<CleverbotResponse> GetCleverbotResponse(string message)
        {
            return await _cleverbot.GetResponseAsync(message);
        }

        private Task _client_Log(LogMessage arg)
        {
            return Task.CompletedTask;
        }

        private void ServieHookappTimer(Object o)
        {
            var serverStats = ApiEndpoint.GetServerStats();
            _serviceHookappStats.Add(serverStats);
            File.WriteAllText(Const.SERVICE_HOOKAPP_HISTORY, JsonConvert.SerializeObject(_serviceHookappStats, Formatting.Indented));
        }

        private void ServerStatusTimer(Object o)
        {
            IRestResponse response = null;
            var watch = Stopwatch.StartNew();
            string exceptionMessage = string.Empty;

            try
            {
                _log.Info($"Pinging {_settings.ServerEndpointStatus} for connection status...");
                var client = new RestClient(new Uri(_settings.ServerEndpointStatus));
                var request = new RestRequest(Method.GET);
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
                _log.Error($"Failed connecting to url. Error: {ex.Message}");
            }

            watch.Stop();
            _log.Info($"Ping took {watch.ElapsedMilliseconds} ms to complete.");

            if (response != null)
            {
                _statusResponseErrors = 0;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _log.Info($"Server ping did not return OK, but instead {response.StatusDescription}");
                    _statusChannel.SendMessageAsync("", false, DiscordMessageFormatter.GetRestResponseMessage(response, _settings.ServerEndpointStatus, watch.ElapsedMilliseconds).Build());
                }
                else
                {
                    _log.Info($"Server ping was successful!");
                }
            }
            else
            {
                _statusResponseErrors++;
                _log.Error($"Server response was null. If this happens {3 - _statusResponseErrors} more time(s) I will notify the Discord channel.");

                if (_statusResponseErrors >= 2)
                {
                    _log.Error($"Server didn't respond 3 times in a row. I will now notify the Discord channel.");
                    _statusChannel.SendMessageAsync("", false, DiscordMessageFormatter.GetRestResponseFailedMessage(exceptionMessage, _settings.ServerEndpointStatus).Build());
                    _statusResponseErrors = 0;
                }
            }
        }

        private T GetDelivery<T>(string jsonBody) where T : new()
        {
            try
            {
                return JsonConvert.DeserializeObject<T>(jsonBody);
            }
            catch (Exception ex)
            {
                _log.Error($"Unable to parse Github delivery of type {typeof(T)}\nException: {ex.Message}");
                return default(T);
            }
        }

        private void GithubDelivery(string eventName, string jsonBody)
        {
            _log.Info($"GitHub webhook delivery for '{eventName}' received.");
            switch (eventName)
            {
                case "push":
                    OnPush(GetDelivery<PushEvent.RootObject>(jsonBody));
                    break;

                case "commit_comment":
                    OnCommitComment(GetDelivery<CommitCommentEvent.RootObject>(jsonBody));
                    break;

                case "issues":
                    OnIssues(GetDelivery<IssuesEvent.RootObject>(jsonBody));
                    break;

                case "project_card":
                    OnProjectCard(GetDelivery<ProjectCardEvent.RootObject>(jsonBody));
                    break;

                case "create":
                    OnCreate(GetDelivery<CreateEvent.RootObject>(jsonBody));
                    break;

                case "deployment":
                    OnDeployment(GetDelivery<DeploymentEvent.RootObject>(jsonBody));
                    break;

                case "issue_comment":
                    OnIssueComment(GetDelivery<IssueCommentEvent.RootObject>(jsonBody));
                    break;

                case "label":
                    OnLabel(GetDelivery<LabelEvent.RootObject>(jsonBody));
                    break;

                case "milestone":
                    OnMilestone(GetDelivery<MilestoneEvent.RootObject>(jsonBody));
                    break;

                default:
                    _log.Error($"I don't have an implementation for the event '{eventName}'");
                    break;
            }
        }

        private void SendEventMessage(EmbedBuilder build)
        {
            _log.Info($"Sending a message to Discord channel with description: {build.Description}");
            _githubChannel.SendMessageAsync("", false, build.Build());
        }

        private void OnPush(PushEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#pushevent
            if (obj == null)
                return;
            
            SendEventMessage(DiscordMessageFormatter.GetOnPushMessage(obj));
        }

        private void OnCommitComment(CommitCommentEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#commitcommentevent
            if (obj == null)
                return;

            SendEventMessage(DiscordMessageFormatter.GetOnCommitCommentMessage(obj));
        }

        private void OnIssues(IssuesEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#issuesevent
            if (obj == null)
                return;
            
            switch (obj.action)
            {
                case "opened":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesOpenedMessage(obj));
                    break;

                case "closed":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesClosedMessage(obj));
                    break;

                case "reopened":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesReopenedMessage(obj));
                    break;

                case "assigned":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesAssignedMessage(obj));
                    break;

                case "unassigned":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesUnassignedMessage(obj));
                    break;

                case "labeled":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesLabeledMessage(obj));
                    break;

                case "unlabeled":
                    SendEventMessage(DiscordMessageFormatter.GetOnIssuesUnlabeledMessage(obj));
                    break;
            }
        }

        private void OnIssueComment(IssueCommentEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#issuecommentevent
            if (obj == null)
                return;

            SendEventMessage(DiscordMessageFormatter.GetOnIssueCommentMessage(obj));
        }

        private async void OnProjectCard(ProjectCardEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#projectcardevent
            if (obj == null)
                return;
            
            ProjectColumn column = await _githubClient.Repository.Project.Column.Get(obj.project_card.column_id);
            Issue issue = null;

            string contentUrl = obj.project_card.content_url;
            if (!string.IsNullOrEmpty(contentUrl))
            {
                int pos = contentUrl.LastIndexOf("/") + 1;
                string strId = contentUrl.Substring(pos, contentUrl.Length - pos);
                
                if (int.TryParse(strId, out int id))
                    issue = await _githubClient.Issue.Get(obj.repository.id, id);
            }

            switch (obj.action)
            {
                case "created":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardCreatedMessage(obj, column));
                    break;

                case "edited":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardEditedMessage(obj));
                    break;

                case "converted":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardConvertedMessage(obj));
                    break;

                case "moved":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardMovedMessage(obj, column, issue));
                    break;

                case "deleted":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardDeletedMessage(obj, issue));
                    break;
            }
        }

        private void OnCreate(CreateEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#createevent
            if (obj == null)
                return;
        }

        private void OnDeployment(DeploymentEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#deploymentevent
            if (obj == null)
                return;
        }

        private void OnLabel(LabelEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#labelevent
            if (obj == null)
                return;

            switch (obj.action)
            {
                case "created":
                    SendEventMessage(DiscordMessageFormatter.GetOnLabelCreatedMessage(obj));
                    break;

                case "edited":
                    SendEventMessage(DiscordMessageFormatter.GetOnLabelEditedMessage(obj));
                    break;

                case "deleted":
                    SendEventMessage(DiscordMessageFormatter.GetOnLabelDeletedMessage(obj));
                    break;
            }
        }

        private void OnMilestone(MilestoneEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#milestoneevent
            if (obj == null)
                return;
        }
    }
}