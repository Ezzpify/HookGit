﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using HookAppDiscord.DataHolders;

namespace HookAppDiscord
{
    class Program
    {
        private static Session _session;

        public static void Main(string[] args)
            => new Program().MainAsync().GetAwaiter().GetResult();

        public async Task MainAsync()
        {
            Directory.CreateDirectory(Const.EXTRA_FOLDER_PATH);
            Directory.CreateDirectory(Const.SERVICE_FOLDER_PATH);
            log4net.Config.XmlConfigurator.Configure();

            Console.Title = "HookGit | Discord utility";
            Console.WriteLine("Loading...");

            var settings = Settings.FromJson(Const.SETTINGS_FILE_PATH);
            if (!String.IsNullOrEmpty(settings.Error))
            {
                Console.WriteLine(settings.Error);
            }
            else
            {
                _session = new Session(settings);
                await _session.Connect();
                await Task.Delay(-1);
            }
        }
    }
}
