using System;
using EasyConsole;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Net;
using System.Text;


namespace Discord_Bots.Pages.CurrencyBots {
    public class ClaimDailies : Page {
        public static List<Token> tokens = new List<Token>();
        public ClaimDailies(Program program, List<Token> _tokens) : base("Daily Claimer",
            program) {
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);
            
            Console.Clear();

            var bots = new Dictionary<string, bool>() {
                {"mekos", false },
                {"owo", true},
                {"tatsu", true },
                {"yui", true },
                {"pokecord", false },
                {"flowers", false }
            };

            var _out = string.Join(',', bots
                .Where(item => item.Value)
                .Select(x => x.Key).ToArray());
            Console.WriteLine("Which bots to send? [seperate by ,]");
            Console.WriteLine($"0: [default, {_out}]");
            for (int i = 0; i < bots.Count; i++) {
                Console.WriteLine($"{i + 1}: {bots.Keys.ElementAt(i)}");
            }
            Console.Write(">");
            
            
            var read = Console.ReadLine();
            if (read.Length == 0)
                Helper.printDefault(_out);
            else {
                bots = bots.ToDictionary(p => p.Key, p => false);
                var numsString = read.Split(',');
                for (int i = 0; i < numsString.Length; i++) {
                    //2 , 3 , 4
                    bots[bots.Keys.ToList()[int.Parse(numsString[i])-1]] = true;
                }
                _out = string.Join(',', bots
               .Where(item => item.Value)
               .Select(x => x.Key).ToArray());
                //Console.WriteLine($"setting options {string.Join(',', numsString)}", Color.Yellow);
                Console.WriteLine($"setting options {_out}", Color.Yellow);
            }


            Helper.ReadConfig();
            if (bots["tatsu"]) {
                for (int i = 0; i < Math.Ceiling((double)tokens.Count / 100); i++) { 
                    var req = await tokens[i].getTokenInfo();
                    if (req != null) {
                        tokens[i].id = req.id;
                    }
                }
            }
            TatsuBot.tokens = tokens;
            double countera = 0, counterb = 0;
            var tasks = new List<Task<bool>>();
            Console.Write($"Claiming dailies [{_out}]:", Color.Green);
            var progress = new ProgressBar();
            foreach (Token token in tokens) {
                progress.Report(counterb / tokens.Count, Color.Green);
                if (countera > Settings.tokenLimit) {
                    countera = 0;
                    await Task.Delay(Settings.botDailyDelay);
                }
                if (counterb % 2 == 0) {
                    tasks.Add(Task.Run(() => token.ClaimDailies(Settings.channels[0], mekos: bots["mekos"],
                        yui: bots["yui"], owo: bots["owo"], tatsu: bots["tatsu"], pokecord:bots["pokecord"],
                        flower:bots["flowers"], delayTime:100)));
                }
                else {
                    tasks.Add(Task.Run(() => token.ClaimDailies(Settings.channels[1], mekos: bots["mekos"],
                        yui: bots["yui"], owo: bots["owo"], tatsu: bots["tatsu"],pokecord: bots["pokecord"],
                        flower: bots["flowers"], delayTime:100)));
                }
                if (bots["tatsu"]) {
                    if (counterb % 100 == 0 && !string.IsNullOrEmpty(tokens[(int)counterb / 100].id))
                        Settings.TatsuId = tokens[(int)counterb / 100].id;
                }
                countera += bots.Count(x => x.Value);
                counterb++;
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
    }
}
