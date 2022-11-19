using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace COD_Discord_Bot.Classes {
    class Code {
        public DateTimeOffset DateReceived { get;  set; }
        public string CodeValue { get;  set; }

        public Code(DateTimeOffset date, string code) {
            this.DateReceived = date;
            this.CodeValue = code;
        }

        private Code() {

        }

        private static Regex regex = new Regex(@"\>([A-Z0-9]{6})<\/em><\/p>");
        public static bool TryParse(DateTimeOffset date, string email, out Code code) {
            code = new Code();
            email = email.Replace("\n", "").Trim();
            var matches = regex.Matches(email);
            if (matches.Count != 1) return false;
            code.DateReceived = date;
            code.CodeValue = matches.First().Groups[1].Value;
            return true;
        }

        public override string ToString() {
            return $"{CodeValue} - Received on {DateReceived.ToReadableFormat()}";
        }
    }
}
