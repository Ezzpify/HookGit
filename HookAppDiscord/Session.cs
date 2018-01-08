using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

using Discord;
using Discord.WebSocket;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord
{
    class Session
    {
        private Settings _settings;
        private DiscordSocketClient _client;

        public Session(Settings settings)
        {
            _settings = settings;

            _client = new DiscordSocketClient();
            _client.Log += _client_Log;
            _client.Ready += _client_Ready;
            _client.MessageReceived += _client_MessageReceived;
        }

        private Task _client_MessageReceived(SocketMessage arg)
        {
            Console.WriteLine($"Received message: {arg.Content}");
            return Task.CompletedTask;
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
                + $" Discord bot for keeping track of GitHub projects\n"
                + " ----------------------------------------------------------------\n\n\n";

            Console.WriteLine(ascii);
            return Task.CompletedTask;
        }

        public async Task Connect()
        {
            await _client.LoginAsync(TokenType.Bot, _settings.Token);
            await _client.StartAsync();
        }

        private Task _client_Log(LogMessage arg)
        {
            return Task.CompletedTask;
        }
    }
}
