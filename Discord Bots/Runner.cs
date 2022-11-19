using System;
using System.Threading;
using System.Threading.Tasks;
using EasyConsole;
namespace Discord_Bots {
    class Runner {
        static Task Main(string[] args) {
            return new MainProgram().Run(CancellationToken.None);
        }
    }
}
