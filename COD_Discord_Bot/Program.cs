using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using COD_Discord_Bot.Classes;
using IniParser;
using IniParser.Model;
using NLog;

namespace COD_Discord_Bot {
    class Program {
        private static IniData data;
        public static DateTime dateStarted;
        static async Task Main(string[] args) {
            dateStarted = DateTime.Now;
            LoadParser();
            LoadLogger();
            var token = data["main"]["token"];
            var bot = new DiscordBot(token);
            await bot.RunBot();
        }

        private static void LoadParser() {
            var config = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "config.ini");
            var parser = new FileIniDataParser();
            data = parser.ReadFile(config);
        }
        
        private static void LoadLogger() {
            var config = new NLog.Config.LoggingConfiguration();
            var logFile = new NLog.Targets.FileTarget("logfile") { FileName = "log.txt" };
            var logConsole = new NLog.Targets.ConsoleTarget("logconsole");
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logFile);
            config.AddRule(LogLevel.Info, LogLevel.Fatal, logConsole);
            LogManager.Configuration = config;
        }
    }
}
