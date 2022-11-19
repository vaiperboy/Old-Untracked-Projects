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
    public class TestPage : Page {
        private List<Token> tokens = new List<Token>();
        private ConcurrentDictionary<string, int> channels = new ConcurrentDictionary<string, int>();
        DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
            LogLevel = LogSeverity.Debug
        });
        private static List<string> msgs = new List<string>();
        public TestPage(Program program, List<Token> _tokens) : base("testtt",
            program) {
            tokens = _tokens;
        }

        [Obsolete]
        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);
            //var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "msgs.txt");
            //client.MessageReceived += Client_MessageReceived;
            //await client.LoginAsync(TokenType.Bot, "NDk3NzY1ODk4MjMzMzE1MzQ4.XUVq5Q.EK_ufRjaIsuzWgwVs8s572QQ0ZY", false);
            //await client.StartAsync();
            //Console.WriteLine("READY", Color.Green);
            //Console.WriteLine("Press any key to stop", Color.Red);
            //Console.WriteLine("Current messages: ", Color.Yellow);
            //Console.ReadKey();

            //using (StreamWriter sw = new StreamWriter(path, false)) {
            //    foreach (var line in msgs) {
            //        await sw.WriteLineAsync(line);
            //    }
            //}
            //Console.WriteLine("Finished saving under " + path);
            //Console.ReadKey();



            //foreach (var token in tokens) {
            //    Console.WriteLine(token.Outh, Color.Green);
            //}
            //changeList(tokens[2]);
            //foreach (var token in tokens) {
            //    Console.WriteLine(token.Outh, Color.Red);
            //}

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "linked.txt");
            await File.WriteAllLinesAsync(path, new string[] { "asd" });
            Console.ReadKey();
        }

        static void changeList(Token _tokens) {
            //foreach (var token in _tokens) {
            //    token.Outh = "123";
            //}
            _tokens.Outh = "123";
        }

        private async Task Client_MessageReceived(SocketMessage arg) {
            if (arg.Author.Id == ulong.Parse("280497242714931202")) {
                msgs.Add(arg.Content);
                Console.Write($"\r{msgs.Count}", Color.Green);
            }
        }

        [Obsolete]
        private async Task<bool> Main(string channel, Token token) {
            try {
                using (DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
                    LogLevel = LogSeverity.Debug
                })) {
                    await client.LoginAsync(TokenType.User, token.token, false);
                    await client.StartAsync();
                    await Task.Delay(2000);
                    await token.SendChannelMessage(channel, ".$");
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
