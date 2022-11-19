using System;
using EasyConsole;
using Flurl.Http;
using Console = Colorful.Console;
using System.Drawing;
using System.Threading.Tasks;
using System.Threading;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net;

namespace Discord_Bots.Pages {
    public class ServerMessageSender : Page {
        private List<Token> tokens = new List<Token>();
        public ServerMessageSender(Program program, List<Token> _tokens) : base("Server Message Sender",
            program) {
            tokens = _tokens;
        }
        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);
            Console.WriteLine("Delay of messages to be sent in ms? [default is 100]");
            Console.Write(">");
            var read = Console.ReadLine();
            var delay = Settings.msgSendDelay;
            if (read.Length == 0)
                Helper.printDefault(delay);
            else
                delay = Helper.ReadNumber(input:read);
            read = Input.ReadString("Channels to post in? (seperate by , for multiple)");
            var channels = Settings.channels;
            if (read.Length == 0)
                Helper.printDefault(string.Join(", ", Settings.channels));
            else {
                if (read.Contains(','))
                    Settings.channels = read.Split(',');
                else
                    Settings.channels = new string[] { read };
            }
            start:
            Console.WriteLine("\nMessage to send boss?");
            Console.Write(">");
            var msg = Console.ReadLine().ReadInput(">");
            var tasks = new List<Task<HttpResponseMessage>>();
            var progress = new ProgressBar();
            Console.Write("Sending messages: ", Color.Yellow);
            double counter = 1;
            var channel = channels[0];
            foreach (Token token in tokens) {
                progress.Report(counter / tokens.Count, Color.Green);
                if (counter % Settings.tokenLimit == 0)
                    await Task.Delay(3000);
                if (counter % 2 == 0)
                    tasks.Add(Task.Run(() => token.SendChannelMessage(channels[0], msg, delay)));
                else
                    tasks.Add(Task.Run(() => token.SendChannelMessage(channels[1], msg, delay)));
                //channel = channels.GetNextElement(currentIndex);
                counter++;
            }
            await Task.WhenAll(tasks);
            progress.Dispose();
            //#if DEBUG
            //for (int i = 0; i < tasks.Count; i++) {
            //    var color = Color.Green;
            //    if (tasks[i].Result.StatusCode != HttpStatusCode.OK)
            //        color = Color.Red;
            //    Console.WriteLine($"{tokens[i].token}: " +
            //        $"Code: {tasks[i].Result.StatusCode}"+
            //        $"{await tasks[i].Result.Content.ReadAsStringAsync()}\n\n", color);
            //}
            //#endif
            var succeeded = tasks.Count((x => x.Result.StatusCode == HttpStatusCode.OK));
            var notSucceeded = tasks.Count - succeeded;
            if (succeeded > 0)
                Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
            if (notSucceeded > 0)
                Console.WriteLine($"Failed with {notSucceeded} tokens", Color.Red);
            goto start;
        }
    }
}
