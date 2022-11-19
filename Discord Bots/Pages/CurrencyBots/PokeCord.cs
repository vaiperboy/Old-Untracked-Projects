using System;
using Console = Colorful.Console;
using System.Linq;
using System.Collections.Generic;
using EasyConsole;
using System.Threading;
using System.Threading.Tasks;
using System.Drawing;

namespace Discord_Bots.Pages.CurrencyBots {
    public class PokeCord : Page{
        private static List<Token> tokens = new List<Token>();
        public PokeCord(Program program, List<Token> _tokens) : base("Pokecord Bot",
            program) {
            tokens = _tokens;
        }

        public override async Task Display(CancellationToken cancellationToken) {
            await base.Display(cancellationToken);

            Helper.ReadConfig(user:false, delay:false);
            var clientId = "264434993625956352";
            var tasks = new List<Task<string>>();
            //Console.WriteLine("Pokecord progress");
            foreach (var token in tokens) {
                //tasks.Add(Task.Run(() => ))
                Console.WriteLine(await token.VoteForBot(clientId), Color.Green);
            }
            //Task.WhenAll
            //var succeeded = tasks
            //    .Count(x => )
        }
    }
}
