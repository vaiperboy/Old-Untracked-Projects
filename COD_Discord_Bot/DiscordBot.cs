using DSharpPlus;
using DSharpPlus.CommandsNext;
using NLog;
using System.Threading.Tasks;
using DSharpPlus.Interactivity;
using System;
using System.Linq;
using System.Collections.Generic;
using System.IO;
using DSharpPlus.Entities;
using System.Diagnostics;
using System.Text;
using COD_Discord_Bot.BotTools.Commands;
using DSharpPlus.Interactivity.Extensions;
using COD_Discord_Bot.Responses;

namespace COD_Discord_Bot {
    public class DiscordBot {
        private DiscordClient client;
        public CommandsNextExtension Commands { get; private set; }
        private Dictionary<string[], string> CensoredWords;
        private string Token { get; set; }
        public AccountsIO Writer { get; private set; }
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private string censoresPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets/censored words.txt");
        public DiscordBot (string token) {
            this.Token = token;
            watcher.Path = System.IO.Path.GetDirectoryName(censoresPath);
            watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
            watcher.Filter = System.IO.Path.GetFileName(censoresPath);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
            RefreshCensores();
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e) {
            //RefreshCensores();
        }

        private void RefreshCensores() {
            this.CensoredWords = new Dictionary<string[], string>();
            foreach (var line in File.ReadLines(censoresPath)) {
                var splitK = line.Split(":");
                CensoredWords.Add(splitK[1].Split(','), splitK[0].Trim());
            }
        }
        public async Task RunBot() {
            this.client = new DiscordClient(new DiscordConfiguration() {
                AutoReconnect = true,
                Token = Token,
                TokenType = TokenType.Bot
            });
            //this.client.Ready += Client_Ready;
            client.Ready += Client_Ready;
            this.client.UseInteractivity(new InteractivityConfiguration() {
                Timeout = TimeSpan.FromSeconds(30)
            });

  
            var commandsConfig = new CommandsNextConfiguration() {
                StringPrefixes = new List<string>() { "." },
                EnableMentionPrefix = true,
                EnableDms = true,
                CaseSensitive = false
            };

            
            Commands = client.UseCommandsNext(commandsConfig);
            Commands.RegisterCommands<CODCommands>();
            Commands.RegisterCommands<ShoppyCommands>();
            Commands.RegisterCommands<GeneralCommands>();
            Commands.RegisterCommands<ManagementCommands>();
            Commands.RegisterCommands<ActivisionCommands>();

            client.MessageCreated += (s, e) => {
                _ = Task.Run(() => BotTools.Events.OnMessage.CheckForCensoring(s, e, CensoredWords));
                _ = Task.Run(() => BotTools.Events.OnMessage.ScanForOrderID(s, e));
                return Task.CompletedTask;
            };


            await client.ConnectAsync();
            try {
                new ShoppyWebhooks(await client.GetGuildAsync((ulong)ServerIds.CodAccounts), 8080)
                .Start();
            } catch (Exception) {

            }
            

            await client.UpdateStatusAsync(new DiscordActivity("Free palestine!!!", ActivityType.Competing));
            await Task.Delay(-1);
        }

        

        private Task Client_MessageCreated(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
            throw new NotImplementedException();
        }

        public string GetAllFootprints(Exception x) {
            var st = new StackTrace(x, true);
            var frames = st.GetFrames();
            var traceString = new StringBuilder();

            foreach (var frame in frames) {
                if (frame.GetFileLineNumber() < 1)
                    continue;

                traceString.Append("File: " + frame.GetFileName());
                traceString.Append(", Method:" + frame.GetMethod().Name);
                traceString.Append(", LineNumber: " + frame.GetFileLineNumber());
                traceString.Append("  -->  ");
            }

            return traceString.ToString();
        }

        private Task Client_Ready(DiscordClient sender, DSharpPlus.EventArgs.ReadyEventArgs e) {
            logger.Info("Bot is on");
            return Task.CompletedTask;
        }

    }
}
