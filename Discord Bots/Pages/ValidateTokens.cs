using System;
using EasyConsole;
using Console = Colorful.Console;
using System.Drawing;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using System.Linq;
using System.IO;
using System.Text;

namespace Discord_Bots.Pages {
    public class ValidateTokens : Page {
        List<Token> tokens = new List<Token>();
        public ValidateTokens(Program program, List<Token> _tokens) : base("Token checker",
            program){
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);

            Console.Write("Checking tokens:", Color.Green);
            var progress = new ProgressBar();
            double counter = 1;
            var working = new List<Token>();
            foreach (var token in tokens) {
                var a = await token.IsWorking();
                if (!a)
                    Console.WriteLine($"{token.token}", Color.Red);
                else
                    working.Add(token);
                progress.Report(counter++ / tokens.Count, Color.Green);
            }

            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "working.txt");
            using (StreamWriter sw = new StreamWriter(path, false)) {
                foreach (var token in working) {
                        await sw.WriteLineAsync(token.token);
                }
            }
            Console.WriteLine("Saved file under name of Working.txt");
        }
    }
}
