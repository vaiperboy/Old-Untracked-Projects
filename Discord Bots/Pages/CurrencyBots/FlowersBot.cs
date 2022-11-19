using System;
using Console = Colorful.Console;
using System.Linq;
using System.Collections.Generic;
using EasyConsole;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace Discord_Bots.Pages.CurrencyBots {
    public class FlowersBot : Page{
        private static List<Token> tokens = new List<Token>();
        public FlowersBot(Program program, List<Token> _tokens) : base("Flowers Bot",
            program) {
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);

            Helper.ReadConfig(user:false, delay:false);
            var clientId = "116275390695079945";
            var tasks = new List<Task<string>>();
            //Console.WriteLine("Pokecord progress");
            foreach (var token in tokens) {
                //tasks.Add(Task.Run(() => ))
                var req = await token.VoteForBot(clientId);
                Console.WriteLine(await req.Content.ReadAsStringAsync(), Color.Green);
            }
            //Task.WhenAll
            //var succeeded = tasks
            //    .Count(x => )
        }
    }
}
