using Newtonsoft.Json.Linq;
using OpenQA.Selenium.Chrome;
using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blizzard_Account_Creator {
    class CaptchaFactory {

        public static void AddCaptchaExtension(ChromeOptions options, string extensionPath) {
            options.AddArgument($"load-extension={extensionPath}");
        }

        private HttpClient client = new HttpClient(new HttpClientHandler {
            UseCookies = false, Proxy = new WebProxy("127.0.0.1", 8888)
        });
        private string Key { get; set; }
        public double Balance { get; }
        public CaptchaFactory(string key) {
            this.Key = key;
            client.BaseAddress = new Uri("https://2captcha.com/");
            var req = client.GetAsync($"res.php?key={this.Key}&action=getbalance").Result;
            if (req.StatusCode == HttpStatusCode.OK) {
                var res = req.Content.ReadAsStringAsync().Result;
                if (double.TryParse(res, out double balance)) {
                    this.Balance = balance;
                } else this.Balance = -1;
            } else this.Balance = -1;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="image"></param>
        /// <param name="angle"></param>
        /// <param name="maxCounter">
        /// MaxCounter is in multiples of 10 seconds. E.g: 5 is 50 seconds
        /// </param>
        /// <returns></returns>
        public async Task<string> SolveRotatedCaptcha(Stream[] images, int angle = 40, int maxCounter = 3) {

            using (var form = new MultipartFormDataContent()) {
                form.Headers.TryAddWithoutValidation("content-type", "multipart/form-data");
                form.Add(new StringContent("1"), "json");
                form.Add(new StringContent(this.Key), "key");
                form.Add(new StringContent("rotatecaptcha"), "method");
                form.Add(new StringContent(angle.ToString()), "angle");

                for (var i = 0; i < images.Length; i++) {
                    form.Add(new StreamContent(images[i]), $"file_{(i + 1)}");
                }

                var req = await client.PostAsync("in.php", form);
                var res = await req.Content.ReadAsStringAsync();
                var json = JObject.Parse(res);
                if ((int)json["status"] == 1) {
                    var token = (string)json["request"];
                    int delay = 10, counter = 1;
                    do {
                        await Task.Delay(delay);
                        req = await client.GetAsync($"res.php?key={this.Key}&action=get&id={token}");
                        res = await req.Content.ReadAsStringAsync();
                        json = JObject.Parse(res);
                        if (req.StatusCode == HttpStatusCode.OK) break;
                        else if (counter++ >= maxCounter) return string.Empty;
                    } while (req.StatusCode != HttpStatusCode.OK);
                    return (string)json["request"];
                }
            }
            return string.Empty;
        }
    }
}
