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

namespace Discord_Bots.Pages.CurrencyBots {
    public class YuiBot : Page {
        private List<Token> tokens = new List<Token>();
        private int succeeded, notSucceeded, insufficient;
        private int pin;
        DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
            LogLevel = LogSeverity.Debug
        });
        public YuiBot(Program program, List<Token> _tokens) : base("Yui bot",
            program) {
            tokens = _tokens;
        }
        public override async Task Display(CancellationToken cancellationToken) {

            await base.Display(cancellationToken);
            this.Main().GetAwaiter().GetResult();
        }

        private async Task Main() {

            client.MessageReceived += Client_MessageReceived;
            client.LoggedIn += Client_LoggedIn;
            //client.Log += Client_Log;
            Console.WriteLine("Logging in...", Color.Yellow);
            await client.LoginAsync(TokenType.Bot, "NDk3NzY1ODk4MjMzMzE1MzQ4.DpuaRg.8bgoeSWadVvxXLfbR4KGiXy6tls", true);

            await client.StartAsync();


            await Task.Delay(2000);

            Console.Write("Yui progress: ", Color.Green);
            var progress = new ProgressBar();
            double counter = 0;
            var tasks = new List<Task<HttpResponseMessage>>();
            var timeWaited = 0;
            foreach (Token token in tokens) {
                progress.Report(counter++ / tokens.Count, Color.Green);
                var a = await token.SendChannelMessage("597488695108567040", $"yui send <@{Settings.id}> 700");
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
                    _ = await token.SendChannelMessage("597488695108567040", pin.ToString());
                last:
                    pin = 0;
                    await Task.Delay(1300);
                }
                else {
                    Console.WriteLine($"{token.token}: possible banned", Color.Red);
                }
            }
            await Task.WhenAll(tasks);
            if (succeeded > 0)
                Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
            if (notSucceeded > 0 || insufficient > 0) {
                Console.WriteLine($"\nFailed with {notSucceeded + insufficient} tokens", Color.Red);
                Console.WriteLine($"\tFailed pin: {notSucceeded}", Color.Yellow);
                Console.WriteLine($"\tInsufficient funds: {insufficient}", Color.Yellow);
            }
            Console.WriteLine("\n\nFinished everything", Color.Red);
            client.Dispose();
            progress.Dispose();
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
            else if (arg.Author.Id == 280497242714931202) {
                //Console.WriteLine($"Yui typed: {arg.Content}", System.Drawing.Color.Yellow);
                if (arg.Content.Contains("Are you sure you wish to send this")) {
                    //Console.WriteLine($"Yui pin: {arg.Content.ExtractNumber_TATSU()}", System.Drawing.Color.Green);
                    int.TryParse(arg.Content.ExtractNumber_YUI(), out pin);
                    //Console.WriteLine($"Set pin to: {pin}", Color.Green);
                }
                if (arg.Content.ToLower().Contains(":white_check_mark:"))
                    succeeded++;
                else if (arg.Content.ToLower().Contains("timed out"))
                    notSucceeded++;
                else if (arg.Content.ToLower().Contains("do not have enough"))
                    insufficient++;
            }
        }
    }
}
