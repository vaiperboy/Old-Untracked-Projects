using Microsoft.Edge.SeleniumTools;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Firefox;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.IO;
using System.IO.Compression;
using System.Threading.Tasks;

namespace Blizzard_Account_Creator.Utils {
    class ProxyManager {

       

        public static string AddProxy(ChromeOptions options, ProxyV2 proxy, string folderPath) {
            string zipPath = Path.Combine(folderPath, $"extension_{Helper.RandomString(6)}.zip"),
                backgroundPath = Path.Combine(folderPath, "background.js"),
                manifestPath = Path.Combine(folderPath, "manifest.json");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            var toReplace = new Dictionary<string, string> {
                {"PROXY_ADDRESS", proxy.Address },
                {"PROXY_PORT", proxy.Port.ToString() },
                {"PROXY_USERNAME", proxy.Username },
                {"PROXY_PASSWORD", proxy.Password },
            };
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create)) {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update)) {
                    ZipArchiveEntry backgroundEntry = archive.CreateEntry(Path.GetFileName(backgroundPath));
                    using (StreamWriter writer = new StreamWriter(backgroundEntry.Open())) {
                        using (StreamReader reader = new StreamReader(backgroundPath)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                writer.WriteLine(line.ReplaceByKeyValuePair(toReplace));
                            }
                        }
                    }
                    archive.CreateEntryFromFile(manifestPath, Path.GetFileName(manifestPath));
                }
            }
            options.AddExtension(zipPath);
            return zipPath;
        }

        public static string AddProxy(EdgeOptions options, ProxyV2 proxy, string folderPath) {
            string zipPath = Path.Combine(folderPath, $"extension_{Helper.RandomString(6)}.zip"),
                backgroundPath = Path.Combine(folderPath, "background.js"),
                manifestPath = Path.Combine(folderPath, "manifest.json");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            var toReplace = new Dictionary<string, string> {
                {"PROXY_ADDRESS", proxy.Address },
                {"PROXY_PORT", proxy.Port.ToString() },
                {"PROXY_USERNAME", proxy.Username },
                {"PROXY_PASSWORD", proxy.Password },
            };
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create)) {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update)) {
                    ZipArchiveEntry backgroundEntry = archive.CreateEntry(Path.GetFileName(backgroundPath));
                    using (StreamWriter writer = new StreamWriter(backgroundEntry.Open())) {
                        using (StreamReader reader = new StreamReader(backgroundPath)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                writer.WriteLine(line.ReplaceByKeyValuePair(toReplace));
                            }
                        }
                    }
                    archive.CreateEntryFromFile(manifestPath, Path.GetFileName(manifestPath));
                }
            }
            options.AddExtension(zipPath);
            return zipPath;
        }

        public static string AddProxy(FirefoxProfile options, ProxyV2 proxy, string folderPath) {
            string zipPath = Path.Combine(folderPath, $"extension_{Helper.RandomString(6)}.zip"),
                backgroundPath = Path.Combine(folderPath, "background.js"),
                manifestPath = Path.Combine(folderPath, "manifest.json");
            if (File.Exists(zipPath)) File.Delete(zipPath);
            var toReplace = new Dictionary<string, string> {
                {"PROXY_ADDRESS", proxy.Address },
                {"PROXY_PORT", proxy.Port.ToString() },
                {"PROXY_USERNAME", proxy.Username },
                {"PROXY_PASSWORD", proxy.Password },
            };
            using (FileStream zipToOpen = new FileStream(zipPath, FileMode.Create)) {
                using (ZipArchive archive = new ZipArchive(zipToOpen, ZipArchiveMode.Update)) {
                    ZipArchiveEntry backgroundEntry = archive.CreateEntry(Path.GetFileName(backgroundPath));
                    using (StreamWriter writer = new StreamWriter(backgroundEntry.Open())) {
                        using (StreamReader reader = new StreamReader(backgroundPath)) {
                            string line;
                            while ((line = reader.ReadLine()) != null) {
                                writer.WriteLine(line.ReplaceByKeyValuePair(toReplace));
                            }
                        }
                    }
                    archive.CreateEntryFromFile(manifestPath, Path.GetFileName(manifestPath));
                }
            }
            options.AddExtension(zipPath);
            return zipPath;
        }

        public static async Task<Proxies> LoadProxies(string path) {
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
