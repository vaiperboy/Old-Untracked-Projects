using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Console = Colorful.Console;
using Flurl.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.Linq;
using System.Text.RegularExpressions;
using NLog;
using HtmlAgilityPack;
using System.IO;
using Discord.WebSocket;
using Discord;
using Color = System.Drawing.Color;
namespace Discord_Bots {
    public static class Helper {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();


        public static IEnumerable<List<T>> splitList<T>(this List<T> locations, int nSize = 30) {
            for (int i = 0; i < locations.Count; i += nSize) {
                yield return locations.GetRange(i, Math.Min(nSize, locations.Count - i));
            }
        }

        public static async Task WriteTokens(this List<Token> _tokens, string path) {
            using (StreamWriter writer = new StreamWriter(path, false)) {
                foreach (var token in _tokens) {
                    var toPrint = token.token;
                    toPrint += !string.IsNullOrEmpty(token.Outh) ? $":{token.Outh}" : string.Empty;
                    await writer.WriteLineAsync(toPrint);
                }
            }
        }

        public static async Task WaitTillTimeReached(this string input, TimeSpan delayTime) {
            var start = DateTime.Now;
            var oldDate = DateTime.Parse(input);
            //var oldDate = DateTime.Parse("19:25:00");
            if ((start - oldDate) < delayTime) {
                var totalTime = delayTime - (start - oldDate);
                Console.WriteLine($"waiting {totalTime.TotalSeconds} seconds");
                await Task.Delay(totalTime);
                Console.WriteLine($"{delayTime.TotalSeconds} seconds have passed on {DateTime.Now}", Color.Green);
            }
        }

        public static TimeSpan DelayTime(this string input) {
            input = input.ToLower();
            if (input.Contains("m")) {
                input = input.Replace("m", string.Empty);
                int.TryParse(input, out int output);

                return TimeSpan.FromMinutes(output);
            }
            else if (input.Contains("s")) {
                input = input.Replace("s", string.Empty);
                int.TryParse(input, out int output);

                return TimeSpan.FromSeconds(output);
            }
            int.TryParse(input, out int _output);
            return TimeSpan.FromMinutes(_output);
        }

        private static readonly Regex sWhitespace = new Regex(@"\s+");
        public static string ReplaceWhiteSpace(this string input, string _new)
            =>  sWhitespace.Replace(input, _new);

        public static string ExtractNumber_TATSU(this string input)
            => input.GetBetween("type `", "` or").Trim();

        //Are you sure you wish to send this? Type 3241 to confirm.
        public static string ExtractNumber_YUI(this string input)
            => input.GetBetween("Type `", "` to").Trim();

        public static int ExtractNumber(this string input) {
            var reg = Regex.Match(input, @"(\d+(?>\.\d+)*)\w+?(\d+)");
            if (reg.Success)
                return int.Parse(reg.Value);
            return 0;
        }

        private static HtmlDocument doc = new HtmlDocument();
        public static async Task<string> GetCsrf(string cookie) {
            //https://discordbots.org/bot/365975655608745985/vote
            var req = await "https://discordbots.org/bot/365975655608745985/vote"
                .WithHeader("accept-language", " en-US,en;q=0.9")
                .WithHeader("cookie", cookie)
                .WithHeader("user-agent", " Mozilla/5.0 (Macintosh; Intel Mac OS X 10_14_5) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/75.0.3770.100 Safari/537.36")
                .GetAsync();
            var res = await req.Content.ReadAsStringAsync();
            doc.LoadHtml(res);
            var script = doc.DocumentNode.Descendants()
                             .Where(n => n.Name == "script")
                             .Last()
                             .InnerText;
            var csrf = script
                .GetBetween("csrf: '", "'");
            return csrf;
        }

        public static string GetBetween(this string content, string startString, string endString) {
            int Start = 0, End = 0;
            if (content.Contains(startString) && content.Contains(endString)) {
                Start = content.IndexOf(startString, 0) + startString.Length;
                End = content.IndexOf(endString, Start);
                return content.Substring(Start, End - Start);
            }
            else
                return string.Empty;
        }

        public static string ValidateInvLink(this string input) {

            //var a = Regex.Match(input, "([A-Za-z0-9]{6})", RegexOptions.IgnoreCase);
            //while (!a.Success) {
            //    Console.WriteLine("Code not valid... try again", Color.Red);
            //    Console.Write(">");
            //    input = Console.ReadLine();
            //    a = Regex.Match(input, "[^a-z0-9]{6}", RegexOptions.IgnoreCase);
            //}
            //return a.Value;

            if (input.Contains("discord", StringComparison.OrdinalIgnoreCase))
                return input.Substring(input.LastIndexOf('/') + 1);
            return input;
             
        }

        public static void ReadConfig(bool user = true, bool channels = true, bool delay = true) {
            if (user) {
                Console.Write("id of your user?");
                var userId = Console.ReadLine();
                if (userId.Length > 0)
                    Settings.id = userId;
                else
                    Helper.printDefault(Settings.id);
            }
            if (channels) {
                Console.Write("\ntype channel ids (spread by ,)");
                var channelsRead = Console.ReadLine();
                if (channelsRead.Length > 0 && channelsRead.Contains(','))
                    Settings.channels = channelsRead.Split(',');
                else
                    Helper.printDefault(string.Join(", ", Settings.channels));
            }
            if (delay) {
                Console.WriteLine("Delay for every  5 users? [in ms, 3000 is default]");
                Console.Write("3000 default> ");
                var read = Console.ReadLine();
                if (string.IsNullOrEmpty(read))
                    Helper.printDefault(Settings.botDailyDelay.ToString());
                else
                    Settings.botDailyDelay = Helper.ReadNumber();
            }
        }

        public static string GetNextElement(this string[] strArray, int index) {
            if ((index > strArray.Length - 1) || (index < 0))
                throw new Exception("Invalid index");

            else if (index == strArray.Length - 1)
                index = 0;

            else
                index++;

            return strArray[index];
        }

        public static async Task<bool> ClaimDailies(this Token token, string channel,
            bool mekos = false,
            bool yui = false,
            bool owo = false,
            bool tatsu = false,
            bool pokecord = false,
            bool flower = false, int delayTime = 0) {
            var results = new List<HttpStatusCode>();
            if (mekos) {
                results.Add((await token.SendChannelMessage(channel, ">daily", delayTime)).StatusCode);
                await Task.Delay(delayTime);
            }

            if (yui) {
                //results.Add((await token.SendChannelMessage(channel, "yui daily", delayTime)).StatusCode);
                results.Add((await token.SendChannelMessage(channel, $"yui daily <@{Settings.id}>", delayTime)).StatusCode);
                await Task.Delay(delayTime);
            }

            if (owo) {
                results.Add((await token.SendChannelMessage(channel, "owo daily", delayTime)).StatusCode);
                await Task.Delay(delayTime);
            }

            if (tatsu) {
                //results.Add((await token.SendChannelMessage(channel, "t!daily", delayTime)).StatusCode);
                results.Add((await token.SendChannelMessage(channel, $"t!daily <@{Settings.TatsuId}>", delayTime)).StatusCode);
                await Task.Delay(delayTime);
            }
            if (pokecord) {
                var clientId = "264434993625956352";
                var req = await token.VoteForBot(clientId);
                if (req.StatusCode == HttpStatusCode.OK) {
                    results.Add((await token.SendChannelMessage(channel, "p!daily claim")).StatusCode);
                }
                results.Add(req.StatusCode);

                //REMOVE
                Console.WriteLine(await req.Content.ReadAsStringAsync(), Color.Green);
                await Task.Delay(delayTime);
            }
            if (flower) {
                var clientId = "116275390695079945";
                var req = await token.VoteForBot(clientId);
                if (req != null) {
                    results.Add(req.StatusCode);
                    //REMOVE
                    //Console.WriteLine(await req.Content.ReadAsStringAsync(), Color.Green);
                }
                await Task.Delay(delayTime);
            }

            return results.All(x => x == HttpStatusCode.OK);

        }

        [Obsolete]
        public static async Task<bool> SendFlowers(this Token token, string channel, string id) {
            bool output;
            using (DiscordSocketClient client = new DiscordSocketClient(new DiscordSocketConfig() {
                LogLevel = LogSeverity.Debug
            })) {
                await client.LoginAsync(TokenType.User, token.token, false);
                await client.StartAsync();
                await Task.Delay(2000);
                output = (await token.SendChannelMessage(channel, $".give all <@{Settings.id}>")).StatusCode == HttpStatusCode.OK;
                await client.LogoutAsync();
            }
            return output;
        }

        public static async Task<bool> SendPokecord(this Token token, Token target, string channel, int amount = 100) {
            //return (await token.SendChannelMessage(channel, $"t!credits <@{id}> {amount}", 100)).StatusCode == HttpStatusCode.OK;
            var list = new List<bool>();
            list.Add((await token.SendChannelMessage(channel, $"p!trade <@{target.id}>")).StatusCode == HttpStatusCode.OK);
            await Task.Delay(600);
            list.Add((await target.SendChannelMessage(channel, $"p!accept")).StatusCode == HttpStatusCode.OK);
            await Task.Delay(500);
            list.Add((await token.SendChannelMessage(channel, $"p!c add {amount}")).StatusCode == HttpStatusCode.OK);
            await Task.Delay(3333);
            list.Add((await token.SendChannelMessage(channel, $"p!confirm")).StatusCode == HttpStatusCode.OK);
            list.Add((await target.SendChannelMessage(channel, $"p!confirm")).StatusCode == HttpStatusCode.OK);
            return list.All(x => x);
        }

        public static async Task<bool> SendTatsu(this Token token, string channel, string id, int amount = 200) {
            return (await token.SendChannelMessage(channel, $"t!credits <@{id}> {amount}", 100)).StatusCode == HttpStatusCode.OK;

        }

        public static async Task<bool> SendYui(this Token token, string channel, string id, int amount = 700) {
            return (await token.SendChannelMessage(channel, $"yui send <@{id}> {amount}", 100)).StatusCode == HttpStatusCode.OK;
        }


        public static async Task<bool> SendMekos(this Token token, string channel, string id, int amount = 100) {
            return (await token.SendChannelMessage(channel, $">give <@{id}> {amount}", 100)).StatusCode == HttpStatusCode.OK;
        }

        public static async Task<bool> SendOwo(this Token token, string channel, string id, int amount = 140) {
            return (await token.SendChannelMessage(channel, $"owo give <@{id}> {amount}", 100)).StatusCode == HttpStatusCode.OK;
        }


        public static string ReadInput(this string input, string message = null) {
            while (input.Length == 0) {
                Console.WriteLine("Message can't be empty you silly..", Color.Red);
                if (!string.IsNullOrEmpty(message))
                    Console.Write($"\n{message}");
                input = Console.ReadLine();
            }
            return input;
        }

        public static async Task<bool> IsWorking(this Token token) {
            var req = await $"https://discordapp.com/api/v6/users/@me"
                .WithHeader("authorization", token.token)
                .AllowAnyHttpStatus()
                .GetAsync();
            var resJson = JObject.Parse(await req.Content.ReadAsStringAsync());
            var msg = (string)resJson["message"];
            if (!string.IsNullOrEmpty(msg))
                return !msg.Contains("Unauthorized");
            return true;
        }

        public static async Task<bool> IsVerified(this Token token) {
            var req = await $"https://discordapp.com/api/v6/users/@me"
                .WithHeader("authorization", token.token)
                .AllowAnyHttpStatus()
                .GetAsync();
            var resJson = JObject.Parse(await req.Content.ReadAsStringAsync());
            return (bool?)resJson["verified"] ?? false;
        }

        public static string GenerateRandomString(int length = 6) {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--) {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }

        public static async Task<Token> getTokenInfo(this Token _token) {

            //https://discordapp.com/api/v7/users/@me

            var req = await $"https://discordapp.com/api/v6/users/@me"
                .WithHeader("accept", "*/*")
                .WithHeader("accept-encoding", "gzip, deflate, br")
                .WithHeader("accept-language", "en-US")
                .WithHeader("content-type", "application/json")
                .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36")
                .WithHeader("authorization", _token.token)
                .AllowAnyHttpStatus()
                .GetAsync();

            if (req.StatusCode == HttpStatusCode.OK) {
                var resJson = JObject.Parse(await req.Content.ReadAsStringAsync());

                return new Token {
                    id = (string)resJson["id"],
                    username = (string)resJson["username"],
                    discriminator = (string)resJson["discriminator"],
                    verified = (bool)resJson["verified"],
                    phone = (string)resJson["phone"],
                    Email = (string)resJson["email"],
                    token = _token.token
                };
            }
            return null;
        }



        public static bool validTokenFormat(this string input) {
            if (input.Length > 0)
                return true;
            return false;
        }

        public static int ReadNumber(int range = -1, string message = "", string input = "") {
            Console.WriteLine("Input numbers only!", Color.Red);
        askNum:
            if (message.Length > 0)
                Console.WriteLine(message);
            Console.Write(">");
            if (string.IsNullOrEmpty(input))
                input = Console.ReadLine();
            var isNum = int.TryParse(input, out int output);
            if (!isNum) {
                Console.WriteLine("Input positive numbers only!", Color.Red);
                Console.Write(">");
                input = Console.ReadLine();
                goto askNum;
            }
            if (range > 0) {
                if (output > range) {
                    Console.WriteLine("Make sure number is within range!", Color.Red);
                    Console.Write(">");
                    input = Console.ReadLine();
                    goto askNum;

                }
            }
            return output;
        }

        public static void printDefault(object input = null) {
            input = !string.IsNullOrEmpty(input.ToString()) ? $"[{input.ToString()}]" : input.ToString();
            Console.WriteLine($"putting default...{input.ToString()}", Color.Yellow);
        }
    }
}
