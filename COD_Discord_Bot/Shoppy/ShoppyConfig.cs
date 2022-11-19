using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Shoppy {
    class ShoppyConfig {
        public TimeSpan Delays { get; set; } = TimeSpan.FromMilliseconds(10);
        public string ShoppyProductToDuplicate;
        public int UpdateRate = 5;
        public ShoppyConfig(string shoppyProductToDuplicate) {
            this.ShoppyProductToDuplicate = shoppyProductToDuplicate;
        }
    }
}
