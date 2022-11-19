using System;
using EasyConsole;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Console = Colorful.Console;
using System.Drawing;
using Newtonsoft.Json.Linq;
using System.Linq;

namespace Discord_Bots.Pages.CurrencyBots {
    public class MekosOwoBot : Page {
        private List<Token> tokens = new List<Token>();
        private int succeeded, notSucceeded;
        public MekosOwoBot(Program program, List<Token> _tokens) : base("Mekos Bot",
            program) {
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);

            Console.Clear();
            Console.WriteLine("Which tasks to do? (0 [default, mekos + owo], 1 for mekos, 2 for owo");
            Console.Write("0 default>");
            var read = Console.ReadLine();
            int tasksNum;
            if (read.Length == 0) {
                tasksNum = 0;
                Helper.printDefault("0");
            }
            else {
                tasksNum = Helper.ReadNumber(3, input: read);
            }
            switch (tasksNum) {
                case 0:
                    Console.WriteLine("Mekos + OWO set", Color.Gold);
                    break;
                case 1:
                    Console.WriteLine("Mekos set", Color.Gold);
                    break;
                case 2:
                    Console.WriteLine("OwO set", Color.Gold);
                    break;
            }
            Console.Write("id of your user?");
            var userId = Console.ReadLine();
            if (userId.Length > 0)
                Settings.id = userId;
            else
                Helper.printDefault(Settings.id);
            askChannel:
            Console.Write("\ntype channel ids (spread by ,) ");
            Console.WriteLine("MUST 2 CHANNELS (not implemneted for rest)", Color.Red);
            var channelsRead = Console.ReadLine();
            if (channelsRead.Length > 0 && channelsRead.Contains(',')) {
                var channelsSplit = channelsRead.Split(',');
                while (channelsSplit.Length != 2) {
                    Console.WriteLine("Please input 2 channels", Color.Red);
                    goto askChannel;
                }
                Settings.channels = channelsSplit;
            }
            else
                Helper.printDefault(string.Join(", ", Settings.channels));
            Console.WriteLine("Delay for every 5 users? [in ms, 3000 is default]");
            Console.Write("3000 default> ");
            read = Console.ReadLine();
            if (read.Length == 0)
                Helper.printDefault(Settings.botDailyDelay.ToString());
            else
                Settings.botSendDelay = Helper.ReadNumber();

            Console.WriteLine("Amount to send? [default is 140]");
            Console.Write(">");
            var amount = 140;
            read = Console.ReadLine();
            if (string.IsNullOrEmpty(read))
                Helper.printDefault(amount);
            else {
                amount = Helper.ReadNumber(input:read);
                Console.WriteLine($"setting {amount}", Color.Yellow);
            }

            Console.Write("Press any button to start", Color.Green);
            Console.ReadLine();
            var tasks = new List<Task<bool>>();
            
            double counter = 0;
            var channel = Settings.channels[0];

            //MEKOS
            if (tasksNum == 0
                || tasksNum == 1) {
                Console.Write($"Mekos progress: ", Color.Green);
                var progress = new ProgressBar();
                foreach (Token token in tokens) {
                    progress.Report(counter / tokens.Count, Color.Green);
                    if (Settings.botSendDelay > 0) {
                        if (counter % Settings.tokenLimit == 0 && counter > 0) {
                            //Console.WriteLine($"\n\nWaiting {Settings.mekosDailyDelay / 1000} seconds for delay....", Color.IndianRed);
                            await Task.Delay(Settings.botSendDelay);
                        }
                    }
                    if (counter % 2 == 0) {
                        tasks.Add(Task.Run(() => token.SendMekos(Settings.channels[0], Settings.id)));
                    }
                    else {
                        tasks.Add(Task.Run(() => token.SendMekos(Settings.channels[1], Settings.id)));
                    }
                    counter++;
                }
                await Task.WhenAll(tasks);
                progress.Dispose();
                succeeded = tasks.Count(x => x.Result);
                notSucceeded = tokens.Count - succeeded;
                if (succeeded > 0) {
                    Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
                    Console.WriteLine($"Amount that was sent: {amount * succeeded}", Color.Green);
                }
                if (notSucceeded > 0)
                    Console.WriteLine($"Failed with {notSucceeded} tokens", Color.Red);
                counter = 0;
                tasks.Clear();
            }

            Console.WriteLine("\n\n\n");
            //OWO
            if (tasksNum == 0
                || tasksNum == 2) {
                Console.Write($"Sending OwO progress: ", Color.Green);
                var progress = new ProgressBar();
                counter = 0;
                foreach (Token token in tokens) {
                    progress.Report(counter / tokens.Count, Color.Green);
                    if (Settings.botSendDelay > 0) {
                        if (counter % Settings.tokenLimit == 0) {
                            await Task.Delay(Settings.botSendDelay);
                        }
                    }

                    if (counter % 2 == 0) {
                        tasks.Add(Task.Run(() => token.SendOwo(Settings.channels[0], Settings.id, amount)));
                    }
                    else {
                        tasks.Add(Task.Run(() => token.SendOwo(Settings.channels[1], Settings.id, amount)));
                    }
                    counter++;
                }
                counter = 0;
                await Task.WhenAll(tasks);
                progress.Dispose();
                succeeded = tasks.Count(x => x.Result);
                notSucceeded = tokens.Count - succeeded;
                tasks.Clear();
                if (succeeded > 0)
                    Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
                if (notSucceeded > 0)
                    Console.WriteLine($"Failed with {notSucceeded} tokens", Color.Red);
            }


            Console.WriteLine(">>>>>Finished all tasks<<<<<", Color.Green);
        }
    }
}
