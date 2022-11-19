using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Blizzard_Account_Creator.Utils {
    class AccountManager {

        //pass-by-refernce <3


        /// <summary>
        /// Made for emails.txt
        /// </summary>
        /// <param name="names"></param>
        /// <param name="emailsPath"></param>
        /// <param name="count"></param>
        public static void InitAccs(ConcurrentQueue<Account> accsQueue, List<string> names, string emailsPath, int count, string country = "KAZ" , string overridePw = "") {
            string[] lines = File.ReadAllLines(emailsPath);
            if (lines.Length == 0) return;
            if (count >= lines.Length || count == -1) count = lines.Length;
            var accFactory = new Account(names);
            for (int i = 0; i < count; i++) {
                if (!string.IsNullOrEmpty(lines[i])) {
                    var split = lines[i].Split(':');
                    if (split.Length == 2) {
                        string email = split[0], pass = split[1];
                        var ogEmail = new EmailProvider(email.Split('@')[1], email, pass);
                        accsQueue.Enqueue(accFactory.GenerateAccount(ogEmail, email, overridePw, 0, country));
                    }
                }
            }
        }
    }
}
