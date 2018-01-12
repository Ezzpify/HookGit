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
using HookAppDiscord.Discord;
using HookAppDiscord.Github.EventHolders;

namespace HookAppDiscord
{
    class Session
    {
        private Settings _settings;
        private DiscordSocketClient _client;
        private CommandService _commands;
        private IServiceProvider _services;

        private Translate _translate;
        private ISocketMessageChannel _translationChannel;

        private WCFServer _wcfServer;
        private ISocketMessageChannel _githubChannel;
        private GitHubClient _githubClient;

        private ISocketMessageChannel _statusChannel;
        private int _statusResponseErrors;

        private CleverbotSession _cleverbot;

        public Session(Settings settings)
        {
            _settings = settings;

            Console.WriteLine("Setting up endpoint urls...");
            ApiEndpoint.ServerStatsUrl = _settings.ServerEndpointStats;

            Console.WriteLine("Setting up translation...");
            _translate = new Translate(settings.AzureToken, settings.TranslateTo);

            Console.WriteLine("Setting up github webhook...");
            GithubWebhookDelivery callback = GithubDelivery;
            _wcfServer = new WCFServer(callback);
            _wcfServer.Start();

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

            var result = await _commands.ExecuteAsync(context, argPos, _services);
            if (!result.IsSuccess)
                await context.Channel.SendMessageAsync(result.ErrorReason);
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

            Timer t = new Timer(ServerStatusTimer, null, 0, (int)TimeSpan.FromMinutes(_settings.StatusPingInterval).TotalMilliseconds);

            Console.Clear();
            Console.WriteLine(ascii);
            return Task.CompletedTask;
        }

        private async Task _client_MessageReceived(SocketMessage arg)
        {
            if (!arg.Author.IsBot)
            {
                string message = _translate.GetTranslatedMessage(arg);
                if (message.Length > 0)
                    await _translationChannel.SendMessageAsync(message);

                if (arg.MentionedUsers.Select(o => o.Id).Contains(_client.CurrentUser.Id))
                {
                    CleverbotResponse response = await _cleverbot.GetResponseAsync(message);
                    await arg.Channel.SendMessageAsync(response.Response);
                }
                else
                {
                    await HandleCommandAsync(arg);
                }
            }
        }

        private async Task<CleverbotResponse> GetCleverbotResponse(string message)
        {
            return await _cleverbot.GetResponseAsync(message);
        }

        private Task _client_Log(LogMessage arg)
        {
            return Task.CompletedTask;
        }

        private void ServerStatusTimer(Object o)
        {
            IRestResponse response = null;
            var watch = Stopwatch.StartNew();
            string exceptionMessage = string.Empty;

            try
            {
                var client = new RestClient(new Uri(_settings.ServerEndpointStatus));
                var request = new RestRequest(Method.GET);
                response = client.Execute(request);
            }
            catch (Exception ex)
            {
                exceptionMessage = ex.Message;
            }

            watch.Stop();
            Console.WriteLine($"ServerEndpointStatus ping took {watch.ElapsedMilliseconds} ms.");

            if (response != null)
            {
                _statusResponseErrors = 0;
                if (response.StatusCode != HttpStatusCode.OK)
                {
                    _statusChannel.SendMessageAsync(DiscordMessageFormatter.GetRestResponseMessage(response, _settings.ServerEndpointStatus, watch.ElapsedMilliseconds));
                }
            }
            else
            {
                if (++_statusResponseErrors >= 3)
                {
                    _statusChannel.SendMessageAsync(DiscordMessageFormatter.GetRestResponseFailedMessage(exceptionMessage, _settings.ServerEndpointStatus));
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
                Console.WriteLine($"Unable to parse Github delivery of type {typeof(T)}\nException: {ex.Message}");
                return default(T);
            }
        }

        private void GithubDelivery(string eventName, string jsonBody)
        {
            switch (eventName)
            {
                case "push":
                    Console.WriteLine("Github delivery event 'push' received.");
                    OnPush(GetDelivery<PushEvent.RootObject>(jsonBody));
                    break;

                case "commit_comment":
                    Console.WriteLine("Github delivery event 'commit_comment' received.");
                    OnCommitComment(GetDelivery<CommitCommentEvent.RootObject>(jsonBody));
                    break;

                case "issues":
                    Console.WriteLine("Github delivery event 'issues' received.");
                    OnIssues(GetDelivery<IssuesEvent.RootObject>(jsonBody));
                    break;

                case "project_card":
                    Console.WriteLine("Github delivery event 'project_card' received.");
                    OnProjectCard(GetDelivery<ProjectCardEvent.RootObject>(jsonBody));
                    break;

                case "create":
                    Console.WriteLine("Github delivery event 'create' received.");
                    OnCreate(GetDelivery<CreateEvent.RootObject>(jsonBody));
                    break;

                case "deployment":
                    Console.WriteLine("Github delivery event 'deployment' received.");
                    OnDeployment(GetDelivery<DeploymentEvent.RootObject>(jsonBody));
                    break;

                case "issue_comment":
                    Console.WriteLine("Github delivery event 'issue_comment' received.");
                    OnIssueComment(GetDelivery<IssueCommentEvent.RootObject>(jsonBody));
                    break;

                case "label":
                    Console.WriteLine("Github delivery event 'label' received.");
                    OnLabel(GetDelivery<LabelEvent.RootObject>(jsonBody));
                    break;

                case "milestone":
                    Console.WriteLine("Github delivery event 'milestone' received.");
                    OnMilestone(GetDelivery<MilestoneEvent.RootObject>(jsonBody));
                    break;
            }
        }

        private void SendEventMessage(string message)
        {
            var builder = new EmbedBuilder()
            {
                Color = Const.DISCORD_EMBED_COLOR,
                Description = message
            };
            
            _githubChannel.SendMessageAsync("", false, builder.Build());
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

        private void OnProjectCard(ProjectCardEvent.RootObject obj)
        {
            //https://developer.github.com/v3/activity/events/types/#projectcardevent
            if (obj == null)
                return;

            switch (obj.action)
            {
                case "created":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardCreatedMessage(obj));
                    break;

                case "edited":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardEditedMessage(obj));
                    break;

                case "converted":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardConvertedMessage(obj));
                    break;

                case "moved":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardMovedMessage(obj));
                    break;

                case "deleted":
                    SendEventMessage(DiscordMessageFormatter.GetOnProjectCardDeletedMessage(obj));
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