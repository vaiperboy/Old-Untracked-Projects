using System;
using EasyConsole;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Threading;
using System.Linq;
using System.Text.RegularExpressions;
using Newtonsoft.Json.Linq;
using System.Net;
using System.Net.Http;
using System.Collections.Concurrent;
using System.IO;
using Flurl.Http;
using Discord.WebSocket;
using Discord;
using Color = System.Drawing.Color;
using IniParser;

namespace Discord_Bots.Pages {
    public class TokenAliver : Page {
        private List<Token> tokens = new List<Token>();
        private ConcurrentDictionary<string, int> channels = new ConcurrentDictionary<string, int>();
        public TokenAliver(Program program, List<Token> _tokens) : base("testtt",
            program) {
            tokens = _tokens;
        }

        [Obsolete]
        public override async Task Display(CancellationToken cancellationToken) {
            var tasks = new List<Task<bool>>();
            double counter = 1;
            Console.Write("Token aliver progress:", Color.Green);
            var progress = new ProgressBar();
            foreach (var token in tokens) {
                if (counter % Settings.tokenLimit == 0) {
                    await Task.Delay(Settings.botSendDelay);
                }
                if (counter % 2 == 0)
                    tasks.Add(Task.Run(() => Main(Settings.channels[0], token)));
                else
                    tasks.Add(Task.Run(() => Main(Settings.channels[1], token)));
                progress.Report(counter++ / tokens.Count, Color.Green);
            }
            await Task.WhenAll(tasks);
            progress.Dispose();
            var succeeded = tasks.Count((x => x.Result));
            var notSucceeded = tasks.Count - succeeded;
            if (succeeded > 0)
                Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
            if (notSucceeded > 0)
                Console.WriteLine($"Failed with {notSucceeded} tokens", Color.Red);
        }

        [Obsolete]
        public async Task<bool> Main(string channel, Token token) {
            try {
                using (DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
                    LogLevel = LogSeverity.Debug
                })) {
                    await client.LoginAsync(TokenType.User, token.token, false);
                    await client.StartAsync();
                    await Task.Delay(2000);
                    var a =  await token.SendChannelMessage(channel, $"p!pick squirtle");
                    if (!a.IsSuccessStatusCode)
                        return false;
                    await client.LogoutAsync();
                }
                return true;
            }
            catch {
                return false;
            }
        }
    }
}
