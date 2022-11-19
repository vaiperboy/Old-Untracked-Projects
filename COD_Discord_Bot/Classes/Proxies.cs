using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Classes {
    public class Proxies : List<ProxyV2> {
        public ProxyV2 PickBestProxy() {
            var proxy = this
                .Where(x => x.IsWorking)
                .OrderBy(x => x.Used)
                .FirstOrDefault();
            lock (proxy) {
                proxy.Used++;
                return proxy;
            }

        }
    }
    public class ProxyV2 {
        public string Address { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public int Used { get; set; }
        public bool IsWorking { get; set; } = true;
        public bool IsAuthed {
            get {
                return !string.IsNullOrEmpty(Username)
                    && !string.IsNullOrEmpty(Password);
            }
        }

        public ProxyV2(string addr, int port) {
            this.Address = addr;
            this.Port = port;

        }

        public ProxyV2(string addr, int port, string username, string pass) {
            this.Address = addr;
            this.Port = port;
            this.Username = username;
            this.Password = pass;
        }

        public ProxyV2() {

        }

        public override string ToString() {
            //--proxy-server=http://user:password@yourProxyServer.com:8080
            return $"http://{this.Username}:{this.Password}@{this.Address}:{this.Port}";
        }
    }

    class ProxyManager {
        public static Proxies LoadProxies(string path) {
            var proxies = new Proxies();
            foreach (var line in File.ReadLines(path)) {
                if (!string.IsNullOrEmpty(line)) {
                    var split = line.Split(':');
                    bool auth = split.Length == 4;
                    if (int.TryParse(split[1], out int port)) {
                        var proxy = new ProxyV2(split[0], port,
                            auth ? split[2] : string.Empty, auth ? split[3] : string.Empty);
                        proxies.Add(proxy);
                    }
                }
            }
            return proxies;
        }
    }
}
