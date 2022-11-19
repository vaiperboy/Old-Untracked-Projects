using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Exceptions {
    class DiscordRuntimeException : Exception {
        public DiscordRuntimeException(string str) : base(str) {

        }
    }
}
