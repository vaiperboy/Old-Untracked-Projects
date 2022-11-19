using System;
using System.Collections.Generic;
using System.Runtime.Loader;
using System.Text;
using System.Linq;

namespace Blizzard_Account_Creator {
    public class Proxies : List<ProxyV2> {
        private static readonly object obj = new object();

        public void Shuffle() {
            var random = new Random((int)System.DateTime.Now.Ticks);
            int n = this.Count;
            while (n > 1) {
                n--;
                int k = random.Next(n + 1);
                var value = this[k];
                this[k] = this[n];
                this[n] = value;
            }
        }

        public ProxyV2 PickBestProxy() {
            lock (obj) {
                var proxy = this
                .Where(x => x.IsWorking)
                .OrderBy(x => x.Used)
                .FirstOrDefault();
                proxy.Used++;
                return proxy;
            }
        }
    }
    public class ProxyV2 {
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set;}
        public string Password { get; set; }
        public int Used { get; set; }
        public bool IsWorking { get; set; } = true;
        public ProxyV2(string addr, int port, string username, string pass) {
            this.Address = addr.Trim();
            this.Port = port;
            this.Username = username.Trim();
            this.Password = pass.Trim();
        }

        public override string ToString() {
            //--proxy-server=http://user:password@yourProxyServer.com:8080
            return $"http://{this.Username}:{this.Password}@{this.Address}:{this.Port}";
        }
    }
}
