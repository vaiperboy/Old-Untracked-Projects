using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Blizzard_Account_Creator.Utils {
    public class ReadWrite {
        private string AccountsPath { get; }
        private string NonVerifiedsPath { get; }
        private static readonly object obj = new object();
        public ReadWrite(string batchesPath, string nonVerifiedPath, DateTime date) {
            this.AccountsPath = System.IO.Path.Combine(batchesPath, date.ToString("MM-dd-yyyy H;mm") + ".json");
            this.NonVerifiedsPath = System.IO.Path.Combine(nonVerifiedPath, date.ToString("MM-dd-yyyy H;mm") + ".json");
            //File.Create(AccountsPath).Close();
        }
        public async Task WriteAccount(Account acc) {
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(acc, Newtonsoft.Json.Formatting.Indented);
            await File.AppendAllTextAsync(this.AccountsPath, json); //auto create file
        }

        public async Task WriteAccount(List<Account> accs) {
            lock (obj) {
                var verified = accs.Where(x => x.IsPhoneVerified);
                var nonVerified = accs.Where(x => !x.IsPhoneVerified);
                //verified
                if (verified.Count() > 0)
                File.WriteAllText(this.AccountsPath, Newtonsoft.Json.JsonConvert.SerializeObject(verified, Newtonsoft.Json.Formatting.Indented));
                //non-verified
                if (nonVerified.Count() > 0)
                File.WriteAllText(this.NonVerifiedsPath, Newtonsoft.Json.JsonConvert.SerializeObject(nonVerified, Newtonsoft.Json.Formatting.Indented));
            }
        }
    }
}
