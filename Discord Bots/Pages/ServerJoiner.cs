using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyConsole;
using Console = Colorful.Console;
using System.Drawing;
using System.Linq;
namespace Discord_Bots.Pages {
    public class ServerJoiner : Page {
       private List<Token> tokens = new List<Token>();
       public ServerJoiner(Program program, List<Token> _tokens) : base ("Server Joiner",
           program) {
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);

            Console.WriteLine("Type in invite code or link (no typos ty)");
            Console.Write(">");
            var code = Console.ReadLine().ReadInput(">").ValidateInvLink();
            Console.WriteLine($"Joining: {code}\n\n", Color.Green);
            var tasks = new List<Task<HttpResponseMessage>>();
            double counter = 1;
            var progress = new ProgressBar();
            Console.Write("\nJoining server progress: ", Color.Green);
            foreach (Token token in tokens) {
                var a = await token.JoinServer(code);
                Console.WriteLine(await a.Content.ReadAsStringAsync(), a.StatusCode == System.Net.HttpStatusCode.OK ? Color.Green : Color.Red);
                progress.Report(counter++ / tokens.Count, Color.Green);
                //tasks.Add(Task.Run(() => token.JoinServer(code)));
            }
            await Task.WhenAll(tasks);
            progress.Dispose();
            var succeeded = tasks.Count(x => x.Result.IsSuccessStatusCode);
            var notSucceeded = tokens.Count - succeeded;

            if (succeeded > 0)
                Console.WriteLine($"\n\nSucceeded with {succeeded} tokens", Color.Green);
            if (notSucceeded > 0)
                Console.WriteLine($"Failed with {notSucceeded} tokens", Color.Red);
            Console.WriteLine("\nPress any key to go to main menu");
            Console.ReadKey();
            await Program.NavigateHome(CancellationToken.None);
        }
    }
}
