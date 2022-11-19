using COD_Discord_Bot.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Exceptions {
    class InvalidAccountException : DiscordRuntimeException {
        public InvalidAccountException(Account acc) : base("Account is not valid to login with: " + acc.ToString()) {

        } 
    }
}
