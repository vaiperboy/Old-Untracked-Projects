using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Classes {
    public class ActivisionAccount : Account {
        public string ActivisionUsername { get; set; }
        public string BlizzardUsername { get; set; }
        public bool IsWorking { get; set; } = true;
        public ActivisionAccount(string email, string pw) : base(email, pw) {

        }

        public override string ToString() {
            return $"[Activision Account] {Email}:{Password}";
        }

        public string GetTrackerLink() {
            //or ternary
            if (string.IsNullOrEmpty(ActivisionUsername)) return string.Empty;
            return $"https://cod.tracker.gg/warzone/profile/atvi/{ActivisionUsername.ReplaceWithEntities()}/overview";
        }

        public override bool Equals(object obj) {
            var _obj = obj as ActivisionAccount;
            if (_obj == null)
                return false;
            return this.GetHashCode().Equals(_obj.GetHashCode());
        }

        public override int GetHashCode() {
            return base.Email.GetHashCode() + base.Password.GetHashCode();
        }
    }
}
