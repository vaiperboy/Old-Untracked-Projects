using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.DiscordClasses {
    [Flags]
    public enum Permissions {
        None = 0,
        Accounts = 1,
        Shoppy = 2,
        Activision = 4,
        All = Accounts | Shoppy | Activision
    }
    class DiscordUser {
        public Permissions Perms { get; set; }
        public string Name { get; set; }
        public ulong ID { get; set; }

        public DiscordUser(string name, ulong id, Permissions perms) {
            this.Name = name;
            this.ID = id;
            this.Perms = perms;
        }

        public bool HasPermission(Permissions perm) {
            return (this.Perms & perm) == perm;
        }

        public override int GetHashCode() {
            return ID.GetHashCode();
        }
    }
}
