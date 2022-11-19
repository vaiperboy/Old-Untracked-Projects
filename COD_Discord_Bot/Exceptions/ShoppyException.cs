using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Exceptions {
    class ShoppyException : DiscordRuntimeException {
        public ShoppyException(string str) : base(str) {

        }
    }
}
