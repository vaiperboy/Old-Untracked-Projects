using System;
using EasyConsole;
using System.Drawing;
using Console = Colorful.Console;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using Discord.WebSocket;
using System.Net.Http;
using Color = System.Drawing.Color;
using System.Linq;
using System.Collections.Concurrent;

namespace Discord_Bots.Pages.CurrencyBots {
    public class TatsuBot : Page {
        public static List<Token> tokens = new List<Token>();
        private int succeeded, notSucceeded, insufficient, pin, credits = -1, total;
        private double counter = 1;
        private ConcurrentDictionary<string, int> channels = new ConcurrentDictionary<string, int>();
        DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
            LogLevel = LogSeverity.Debug
        });
        public TatsuBot(Program program, List<Token> _tokens) : base("Tatsu bot",
            program) {
            tokens = _tokens;
        }
        public override async Task Display(CancellationToken cancellationToken) {

            await base.Display(cancellationToken);
            this.Main().GetAwaiter().GetResult();
        }

        private async Task<int> GetTatsu(Token token) {
            await token.SendChannelMessage(Settings.channels[0], "t!credits");
            var timeWaited = 0;
            while (credits == -1) {
                if (timeWaited > 4000) {
                    //Console.WriteLine("I timed out... going to next token", Color.Red);
                    timeWaited = 0;
                    return 0;
                }
                //Console.WriteLine("waiting on pin", Color.Yellow);
                await Task.Delay(300);
                timeWaited += 300;
            }
            await Task.Delay(6100);
            total += credits;
            return credits;
        }

        private async Task Main() {

            //Settings.channels.Tatsu.ToList().ForEach(f => channels.TryAdd(f, 0));
            client.MessageReceived += Client_MessageReceived;
            client.LoggedIn += Client_LoggedIn;
            //client.Log += Client_Log;
            Console.WriteLine("Logging in...", Color.Yellow);
            await client.LoginAsync(TokenType.Bot, "NDk3NzY1ODk4MjMzMzE1MzQ4.DpuaRg.8bgoeSWadVvxXLfbR4KGiXy6tls", false);

            await client.StartAsync();

            Helper.ReadConfig();

            await Task.Delay(2000);

            for (int i = 0; i < Math.Ceiling((double)tokens.Count / 100); i++) {
                var req = await tokens[i].getTokenInfo();
                if (req != null) {
                    tokens[i].id = req.id;
                }
            }

            Console.Write("Tatsu progress: ", Color.Green);
            var progress = new ProgressBar();
            var tasks = new List<Task<HttpResponseMessage>>();
            var timeWaited = 0;
            var toLoop = Math.Ceiling((double)tokens.Count / 100);
            for (int i = 0; i <  toLoop; i++) {
                var tatsu = await GetTatsu(tokens[i]);
                if (tatsu > 0) {
                    var a = await tokens[i].SendChannelMessage(Settings.channels[0], $"t!credits <@{Settings.id}> {tatsu}");
                    if (a.StatusCode == System.Net.HttpStatusCode.OK) {
                        while (pin == 0) {
                            if (timeWaited > 4000) {
                                //Console.WriteLine("I timed out... going to next token", Color.Red);
                                timeWaited = 0;
                                goto last;
                            }
                            //Console.WriteLine("waiting on pin", Color.Yellow);
                            await Task.Delay(300);
                            timeWaited += 300;
                        }
                        await tokens[i].SendChannelMessage(Settings.channels[0], pin.ToString());
                    last:
                        pin = 0;
                        await Task.Delay(1600);
                    }
                    else {
                        Console.WriteLine($"{tokens[i].token}: possible banned", Color.Red);
                    }
                }
                progress.Report(counter / toLoop, Color.Green);
                counter++;
            }
            await Task.WhenAll(tasks);
            progress.Dispose();
            if (total > 0)
                Console.WriteLine($"Amount sent: {total}", Color.Green);
            if (succeeded > 0)
                Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
            if (notSucceeded > 0 || insufficient > 0) {
                Console.WriteLine($"Failed with {notSucceeded + insufficient} tokens", Color.Red);
                Console.WriteLine($"\tFailed pin: {notSucceeded}", Color.Yellow);
                Console.WriteLine($"\tInsufficient funds: {insufficient}", Color.Yellow);
            }
            Console.WriteLine("\n\nFinished everything", Color.Red);
            client.Dispose();
            Console.ReadKey();

            await Task.Delay(-1);
        }

        private async Task Client_Log(LogMessage arg) {
            Console.WriteLine($">>>>>>>>>>>>>>>>{arg.Message}");
        }

        private async Task Client_LoggedIn() {
            Console.WriteLine("Logged in\n\n\n", Color.Green);
        }

        private async Task Client_MessageReceived(SocketMessage arg) {
            //Console.WriteLine($"{arg.Author.Id} typed: {arg.Content}", Color.Red);
            if (arg.Author.Id == ulong.Parse(Settings.id)) {
                //Console.WriteLine($"Toxic typed: {arg.Content}", System.Drawing.Color.Red);
            }
            else if (arg.Author.Id == 172002275412279296) {
                //Console.WriteLine($"Tatsu typed: {arg.Content}", System.Drawing.Color.Yellow);
                if (arg.Content.Contains("balance of")) {
                    credits = arg.Content.Split(new string[] { "balance of" }, StringSplitOptions.None)[1].ExtractNumber();
                    //Console.WriteLine($"Tatsu value: {credits}\n\n", Color.Red);
                }
                
                if (arg.Content.Contains("Credit Transfer")) {
                    var key = int.TryParse(arg.Content.ExtractNumber_TATSU(), out int _pin);
                    if (key)
                        pin = _pin;
                    else
                        pin = -1;
                    //Console.WriteLine($"Set pin to: {pin}", Color.Green);
                }
                if (arg.Content.Contains(":yes:"))
                    succeeded++;
                else if (arg.Content.Contains(":no"))
                    notSucceeded++;
                else if (arg.Content.Contains("insufficient credits"))
                    insufficient++;
            }
        }
    }
}
