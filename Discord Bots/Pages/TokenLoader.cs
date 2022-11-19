using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using EasyConsole;
using Console = Colorful.Console;
using System.Drawing;
using System.IO;

namespace Discord_Bots.Pages {
    class TokenLoader : MenuPage {
        


        public class Local : Page {
            public Local(Program program) : base ("Local .txt tokens loader",
                program) {
            }
            public override async Task Display(CancellationToken cancellationToken) {
                await base.Display(cancellationToken);
                var path = AppDomain.CurrentDomain.BaseDirectory;
                Console.WriteLine("Load tokens from .txt");
                var _tokens = Discord_Bots.TokenLoader.Local.getTokens(out bool isJson, path);
                //if (File.Exists(Path.Combine(path, "config.t")))
                MainProgram.tokens = _tokens;
                Console.WriteLine($"{_tokens.Count} loaded\n" +
                    $"press any button to go to main page", Color.Green);
                Console.ReadKey();
                Console.Clear();
                Console.WriteLine($"{_tokens.Count} loaded\n", Color.Green);

                
                //await Program(cancellationToken);
            }
        }
        
        public TokenLoader(Program program) : base ("Token Loader", 
            program,
            new Option("Local", program.NavigateTo<Local>)) {

        }
    }
}
