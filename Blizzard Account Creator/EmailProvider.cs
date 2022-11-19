using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Blizzard_Account_Creator {
    public class EmailProvider {
        public string Provider { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public static EmailProvider Empty = new EmailProvider();
        public EmailProvider(string prov, string email, string pass) {
            this.Provider = prov;
            this.Email = email;
            this.Password = pass;
        }
        public EmailProvider() {

        }
    }
}
