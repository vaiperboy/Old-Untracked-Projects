using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Exceptions {
    class TimeoutException : DiscordRuntimeException {
        public TimeoutException() : base("Tried max retries but never worked!") {

        }

        public TimeoutException(string str) : base(str) {

        }
    }
}
