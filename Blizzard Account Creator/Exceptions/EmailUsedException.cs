using System;
using System.Collections.Generic;
using System.Text;

namespace Blizzard_Account_Creator.Exceptions {
    class EmailUsedException : Exception {
        public string Email { get; private set; }
        public EmailUsedException(string email) : base("Email used " + email) {
            this.Email = email;
        }
    }
}
