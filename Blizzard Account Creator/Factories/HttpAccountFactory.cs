using Flurl.Util;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Blizzard_Account_Creator {
    class HttpAccountFactory {
        private HttpClient client;
        private CookieContainer container = new CookieContainer();
        public HttpAccountFactory(WebProxy proxy) {
            this.client = new HttpClient(new HttpClientHandler {
                Proxy = proxy, UseCookies = true, AllowAutoRedirect = true, CookieContainer = container
            });
            client.BaseAddress = new Uri("https://eu.battle.net/");
        }

        public async Task<bool> TryCreateAccount(Account account) {
            var req = await client.GetAsync("account/creation/en-us/"); //to get some cookies
            var csrf = await GenerateCSRF();
            var cookies = container.GetCookies(client.BaseAddress);
            if (!string.IsNullOrEmpty(csrf)) {
                var captcha = "3985ec56ffd37c849.0400508301|r=us-east-1|metabgclr=%23ffffff|guitextcolor=%23000000|metaiconclr=%23757575|meta=3|pk=E8A75615-1CBA-5DFF-8032-D16BCF234E10|at=40|atp=2|cdn_url=https://cdn.arkoselabs.com/fc|lurl=https://audio-us-east-1.arkoselabs.com|surl=https://blizzard-api.arkoselabs.com";
                var payload = new Dictionary<string, string> {
                    {"csrftoken", csrf },
                };
                payload = payload.Concat(account.AsHeaders()).ToDictionary(x => x.Key, x => x.Value);
                payload = payload.Concat(new Dictionary<string, string> {
                    {"agreedToPrivacyPolicy", "on" },
                    {"arkoseLabsSessionToken", captcha },
                    {"onReadyMs", "1333"},
                    {"onShownMs", "1533" }
                }).ToDictionary(x => x.Key, x => x.Value);
                
                using (var msg = new HttpRequestMessage(HttpMethod.Post, new Uri("https://eu.battle.net/account/creation/en-us/tos.html"))) {
                    using (var content = new FormUrlEncodedContent(payload)) {
                        msg.Content = content;
                        Helper.AddHeaders(msg);
                        req = await client.SendAsync(msg);
                        var res = await req.Content.ReadAsStringAsync();
                    }
                }
            }

            return true;
        }

        private async Task<string> GenerateCSRF() {
            var requestMsg = new HttpRequestMessage(HttpMethod.Post, new Uri("https://eu.battle.net/login/csrf-token"));
            requestMsg.Headers.TryAddWithoutValidation("Accept", "*/*");
            requestMsg.Headers.TryAddWithoutValidation("Referer" ,"https://eu.battle.net/account/creation/en-us/");
            requestMsg.Headers.TryAddWithoutValidation("Accept", "*/*");
            requestMsg.Headers.TryAddWithoutValidation("Accept", "*/*");
            //requestMsg.Headers.TryAddWithoutValidation("Accept-Encoding" , "gzip, deflate, br");
            requestMsg.Headers.TryAddWithoutValidation("Accept-Language" ,"en-US,en;q=0.9");
            requestMsg.Headers.TryAddWithoutValidation("X-Requested-With", "XMLHttpRequest");
            requestMsg.Headers.TryAddWithoutValidation("Content-length" ,"0");
            requestMsg.Headers.TryAddWithoutValidation("User-Agent" ,"Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/81.0.4044.138 Safari/537.36");
            var req = await client.SendAsync(requestMsg);
            if (req.IsSuccessStatusCode) {
                var buffer = await req.Content.ReadAsByteArrayAsync();
                var byteArray = buffer.ToArray();
                var responseString = Encoding.UTF8.GetString(byteArray, 0, byteArray.Length);
                return responseString;
            }
            return string.Empty;
        }
    }
}
