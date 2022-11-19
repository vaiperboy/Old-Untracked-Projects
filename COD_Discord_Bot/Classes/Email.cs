using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Classes {
    public class Account {
        public string Email { get; set; }
        public string Password { get; set; }
        public Account(string email, string pw) {
            this.Email = email.Trim();
            this.Password = pw.Trim();
        }

        public override string ToString() {
            return $"{Email}:{Password}";
        }
    }

    //incase i add some stuff later
    class Email : Account {
        public string TLD;
        public Email(string email, string pw) : base(email, pw) {
            this.TLD = email.Split("@")[1];
        }
    }
}
