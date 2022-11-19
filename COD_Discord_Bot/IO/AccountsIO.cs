using COD_Discord_Bot.Responses;
using NLog;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace COD_Discord_Bot {
    public class AccountsIO {
        public string Path { get; set; }
        public List<string> Accounts { get; private set; }
        public int Count { get {
                return this.Accounts.Count;
            } }
        private FileSystemWatcher watcher = new FileSystemWatcher();
        private static readonly Logger logger =  LogManager.GetCurrentClassLogger();
        public AccountsIO(string path) {
            this.Path = path;
            ReadEmails().GetAwaiter();
            watcher.Path = System.IO.Path.GetDirectoryName(path);
            watcher.NotifyFilter = NotifyFilters.Size | NotifyFilters.LastWrite;
            watcher.Filter = System.IO.Path.GetFileName(path);
            watcher.Changed += Watcher_Changed;
            watcher.EnableRaisingEvents = true;
        }

        private void Watcher_Changed(object sender, FileSystemEventArgs e) {
            ReadEmails().GetAwaiter();
            logger.Info($"Accounts updated.. new count: {Accounts.Count}");
        }

        public async Task<ReadWriteResponse> GetEmail(int count = 1, bool delete = true) {
            watcher.EnableRaisingEvents = false;
            await ReadEmails(); //probably will change later
            if (Count >= count) {
                var returnAccs = this.Accounts.Take(count).ToList();
                this.Accounts = this.Accounts.Skip(count).ToList();
                if (delete) await File.WriteAllLinesAsync(this.Path, this.Accounts);
                watcher.EnableRaisingEvents = true;
                return new ReadWriteResponse(true, string.Join("\n", returnAccs));
            }
            watcher.EnableRaisingEvents = true;
            return new ReadWriteResponse(false, $"Not enough accounts");
        }

        private async Task ReadEmails() {
            this.Accounts = (await File.ReadAllLinesAsync(this.Path))
                .Where(x => !string.IsNullOrEmpty(x)).ToList();
        }

        public override string ToString() {
            return $"[{System.IO.Path.GetFileNameWithoutExtension(this.Path)}] Current stock: {this.Accounts.Count}";
        }
    }
}
