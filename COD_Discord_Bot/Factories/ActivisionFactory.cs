using COD_Discord_Bot.Classes;
using COD_Discord_Bot.Responses;
using COD_Discord_Bot.Utils;
using Newtonsoft.Json.Linq;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Factories {
    class ActivisionFactory : IDisposable {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        private HttpClient Client;
        private CookieContainer _cookies = new CookieContainer();
        private string CookiesString = string.Empty;
        private ProxyV2 proxy;
        private HttpClientHandler handler;
        private string authHeader, userAgent;

        public CookieContainer Cookies {
            get {
                return _cookies;
            }
            set {
                this._cookies = value;
                handler = new HttpClientHandler() {
                    UseCookies = true,
                    CookieContainer = _cookies,
                    AllowAutoRedirect = false
                };
                
                if (proxy != default) {
                    var _proxy = new WebProxy(proxy.Address, proxy.Port);
                    if (proxy.IsAuthed) _proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
                    handler.Proxy = _proxy;
                }
                this.Client = new HttpClient(handler);
                this.Client.BaseAddress = new Uri("https://profile.callofduty.com");
            }
        }

        public bool IsLoggedIn {
            get {
                return this.Cookies != null &&
                    this.Cookies.GetCookies(Client.BaseAddress) != null;
            }
        }

        private ActivisionAccount account;

        public ActivisionFactory(ActivisionAccount acc, ProxyV2 proxy = default) {
            this.userAgent = "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko";

            this.proxy = proxy;
            this.account = acc;
            handler = new HttpClientHandler() {
                UseCookies = true,
                CookieContainer = _cookies,
                AllowAutoRedirect = false
            };
            if (proxy != default) {
                var _proxy = new WebProxy(proxy.Address, proxy.Port);
                if (proxy.IsAuthed) _proxy.Credentials = new NetworkCredential(proxy.Username, proxy.Password);
                handler.Proxy = _proxy;
            }
            this.Client = new HttpClient(handler);
            this.Client.BaseAddress = new Uri("https://profile.callofduty.com");
            this.Client.DefaultRequestHeaders.TryAddWithoutValidation("user-agent", userAgent);
            this.Client.DefaultRequestHeaders.TryAddWithoutValidation("x_cod_device_id", userAgent);
            ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;
        }

        private ActivisionFactory() {

        }

        public void Dispose() {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (disposing) {
                this.Client.Dispose();
                this.handler.Dispose();
            }
        }

        public async Task<bool> GetUsernames(int maxRetries = 2) {
            if (IsLoggedIn) {
                int retry = 0;
                while (retry++ < maxRetries) {
                    var xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
                    int retries = 0;
                    while (xsrf == null) {
                        _ = await this.GetCSRF();
                        xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
                        if (retries++ >= maxRetries)
                            return false;
                    }
                    logger.Debug($"Trying to get ActivisionID for {account}");
                    var uri = new Uri("https://s.activision.com/activision/profile");
                    using (var reqMsg = new HttpRequestMessage(HttpMethod.Get, uri)) {
                        reqMsg.Headers.Add("cookie", this.CookiesString);
                        var req = await Client.SendAsyncRedirect(reqMsg);
                        if (req.RequestMessage.RequestUri.ToString().Contains("login")) {
                            await Login();
                            req.Dispose();
                            return await GetUsernames();
                        } else if (req.RequestMessage.RequestUri.ToString().Contains("")) {

                        }
                        if (req.IsSuccessStatusCode) {
                            var res = await req.Content.ReadAsStringAsync();
                            var doc = new HtmlAgilityPack.HtmlDocument();
                            doc.LoadHtml(res);
                            var activisionElm = doc.GetElementbyId("forum-name");///
                            var blizzElm = doc.DocumentNode.Descendants("span")
                                .Where(x => x.HasClass("linked-text"))
                                .Where(x => {
                                    var split = x.InnerText.Trim().Split(':');
                                    return split.Length == 2 && split[1].Length > 2;
                                })
                                .FirstOrDefault();
                            if (activisionElm == default && blizzElm == default) continue;
                            if (activisionElm != default) {
                                var activisionId = activisionElm.GetAttributeValue("value", "");
                                logger.Info($"ActivisionID retrireved for account {account} - {activisionId}");
                                account.ActivisionUsername = activisionId;
                            }

                            if (blizzElm != null) {
                                var blizzUsername = blizzElm.InnerHtml.Trim().Split(':')[1];
                                logger.Info($"Blizzard username retrireved for account {account} - {blizzUsername}");
                                account.BlizzardUsername = blizzUsername;
                            }
                            req.Dispose();
                            if (activisionElm != default || blizzElm != default)
                                return true;
                            req.Dispose();
                        }
                    }
                }
                logger.Error($"Couldn't get info for account {account}");
                return false;
            } else {
                int retries = 0;
                await Login();
                return await GetUsernames(maxRetries);
            }
        }

        public async Task<bool> SubmitMissingProfile(string firstName, string lastName) {
            var uri = new Uri("https://s.activision.com/activision/profileEmail");
            var xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
            int maxRetries = 3, retries = 0;
            while (xsrf == null) {
                _ = await this.GetCSRF();
                xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
                if (retries++ >= maxRetries)
                    return false;
            }
            var payload = new List<KeyValuePair<string, string>>() {
                {new KeyValuePair<string, string>("firstName", firstName)},
                {new KeyValuePair<string, string>("lastName", lastName)},
                {new KeyValuePair<string, string>("register", "register")},
                {new KeyValuePair<string, string>("_csrf", xsrf.Value)},
            };
            var _uri = new Uri("https://s.activision.com/activision/missingProfileInformation");
            bool success = false;
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, _uri)) {
                using (var content = new FormUrlEncodedContent(payload)) {
                    reqMsg.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    reqMsg.Headers.TryAddWithoutValidation("Cache-Control", "max-age=0");
                    reqMsg.Headers.TryAddWithoutValidation("Upgrade-Insecure-Requests", "1");
                    reqMsg.Headers.TryAddWithoutValidation("cookie", this.CookiesString);
                    reqMsg.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.83 Safari/537.36 Edg/85.0.564.44");
                    reqMsg.Headers.TryAddWithoutValidation("X-Requested-With", "XMLXhttpRequest");
                    reqMsg.Headers.TryAddWithoutValidation("Accept", "text/html,application/xhtml+xml,application/xml;q=0.9,image/webp,image/apng,*/*;q=0.8,application/signed-exchange;v=b3;q=0.9");
                    reqMsg.Headers.TryAddWithoutValidation("Accept-Encoding", "application/json, text/javascript, */*; q=0.01");
                    reqMsg.Headers.TryAddWithoutValidation("Accept-Language", "en-US,en;q=0.9");
                    reqMsg.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    reqMsg.Headers.TryAddWithoutValidation("Host", "profile.callofduty.com");
                    reqMsg.Headers.TryAddWithoutValidation("Origin", "https://profile.callofduty.com");
                    reqMsg.Headers.TryAddWithoutValidation("Referer", "https://profile.callofduty.com/cod/missingProfileInformation");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "document");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "navigate");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-User", "?1");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");
                    reqMsg.Content = content;
                    var req = await this.Client.SendAsyncRedirect(reqMsg);
                    if (req.RequestMessage.RequestUri.ToString().Contains("sessionExpired=true")) {
                        logger.Debug($"TRYING TO FILL NAME FOR {account.Email}");
                        await Login();
                        return await SubmitMissingProfile(firstName, lastName);
                    }
                    success = req.IsSuccessStatusCode;
                }
            }
            return success;
        }


        public async Task<ActivisionResponse> ChangeEmail(string newEmail) {
            if (!IsLoggedIn) await Login();
            var uri = new Uri("https://s.activision.com/activision/profileEmail");
            var xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
            while (xsrf == null) {
                _ = await this.GetCSRF();
                xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
            }

            var payload = new List<KeyValuePair<string, string>>() {
                {new KeyValuePair<string, string>("email", account.Email)},
                {new KeyValuePair<string, string>("_csrf", xsrf.Value)},
                {new KeyValuePair<string, string>("reenterPassword", account.Password)},
                {new KeyValuePair<string, string>("_csrf", xsrf.Value)},
            };
            logger.Debug("Trying to change email of " + account.Email);
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, uri)) {
                using (var content = new FormUrlEncodedContent(payload)) {
                    reqMsg.Headers.TryAddWithoutValidation("Content-Type", "application/x-www-form-urlencoded");
                    reqMsg.Headers.TryAddWithoutValidation("cookie", this.CookiesString);
                    reqMsg.Headers.TryAddWithoutValidation("user-agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/84.0.4147.89 Safari/537.36");
                    reqMsg.Headers.TryAddWithoutValidation("X-XSRF-TOKEN", xsrf.Value);
                    reqMsg.Headers.TryAddWithoutValidation("X-Requested-With", "XMLXhttpRequest");
                    reqMsg.Headers.TryAddWithoutValidation("Accept", "application/json, text/javascript, */*; q=0.01");
                    reqMsg.Headers.TryAddWithoutValidation("Accept-Encoding", "application/json, text/javascript, */*; q=0.01");
                    reqMsg.Headers.TryAddWithoutValidation("Accept-Language", "en-GB,en-US;q=0.9,en;q=0.8");
                    reqMsg.Headers.TryAddWithoutValidation("Connection", "keep-alive");
                    reqMsg.Headers.TryAddWithoutValidation("Host", "s.activision.com");
                    reqMsg.Headers.TryAddWithoutValidation("Origin", "https://s.activision.com");
                    reqMsg.Headers.TryAddWithoutValidation("Referer", "https://s.activision.com/activision/profile");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Dest", "empty");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Mode", "cors");
                    reqMsg.Headers.TryAddWithoutValidation("Sec-Fetch-Site", "same-origin");

                    reqMsg.Content = content;
                    var req = await this.Client.SendAsyncRedirect(reqMsg);
                    logger.Debug($"Sent request to change email of " + account.Email);
                    var res = await req.Content.ReadAsStringAsync();
                    if (req.IsSuccessStatusCode) {
                        logger.Debug("Changed email of " + account.Email + " to " + newEmail);
                        return new ActivisionResponse(true, "Changed email");
                    }
                    return new ActivisionResponse(false, "Couldn't login to account... " + res);
                }
            }
        }


        public async Task<ActivisionResponse> Skip2FA(int maxRetries = 3) {
            var xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
            int retry = 0;
            while (xsrf == null) {
                _ = await this.GetCSRF(maxRetries);
                xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
                if (retry++ >= maxRetries)
                    return new ActivisionResponse(false, "FAILED TO GET CSRF");
            }
            //reqMsg.Headers.Add("cookie", this.CookiesString);
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, new Uri("https://s.activision.com/static/2ea85c5e127ti17360cc0f1d21e799f4f"))) {
                reqMsg.Headers.Add("cookie", this.CookiesString);
                reqMsg.Content = new StringContent("{\"sensor_data\":\"7a74G7m23Vrp0o5c9113891.66-1,2,-94,-100,Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/87.0.4280.67 Safari/537.36,uaend,12147,20030107,en-US,Gecko,4,0,0,0,395208,9358945,2498,1440,2560,1440,1160,1342,2502,,cpen:0,i1:0,dm:0,cwen:0,non:1,opc:0,fc:0,sc:0,wrc:1,isc:0,vib:1,bat:1,x11:0,x12:1,8283,0.647337640323,803114679475,1,loc:-1,2,-94,-101,do_en,dm_en,t_en-1,2,-94,-105,-1,2,-94,-102,0,-1,0,0,2108,2108,0;-1,2,-94,-108,-1,2,-94,-110,0,1,8098,1144,537;1,1,8098,1144,537;2,1,8098,1133,536;3,1,8099,1118,534;4,1,8100,1107,531;5,1,8101,1096,529;6,1,8102,1084,526;7,1,8103,1073,524;8,1,8104,1062,522;9,1,8105,1052,519;10,1,8106,1040,517;11,1,8107,1028,515;12,1,8108,1015,513;13,1,8109,1004,510;14,1,8112,982,506;15,1,8112,972,504;16,1,8113,961,502;17,1,8114,950,500;18,1,8115,939,498;19,1,8116,926,495;20,1,8117,915,493;21,1,8118,905,491;22,1,8119,894,490;23,1,8120,884,488;24,1,8122,874,486;25,1,8122,864,484;26,1,8123,854,482;27,1,8125,844,480;28,1,8125,831,478;29,1,8126,822,477;30,1,8127,812,475;31,1,8128,802,474;32,1,8129,794,472;33,1,8130,785,471;34,1,8131,775,468;35,1,8132,766,468;36,1,8133,758,466;37,1,8134,746,464;38,1,8135,737,463;39,1,8136,728,462;40,1,8137,720,460;41,1,8138,711,459;42,1,8139,703,458;43,1,8140,695,457;44,1,8141,687,456;45,1,8142,676,454;46,1,8143,669,453;47,1,8144,661,452;48,1,8145,653,451;49,1,8146,646,451;50,1,8147,638,450;51,1,8148,631,449;52,1,8149,624,448;53,1,8150,616,447;54,1,8151,606,446;55,1,8152,600,446;56,1,8153,593,445;57,1,8154,586,444;58,1,8155,579,443;59,1,8156,572,443;60,1,8157,567,442;61,1,8158,560,441;62,1,8159,552,440;63,1,8160,546,440;64,1,8161,540,439;65,1,8162,534,438;66,1,8163,528,438;67,1,8164,523,437;68,1,8168,508,435;69,1,8168,499,434;70,1,8170,491,434;71,1,8171,485,432;72,1,8172,481,432;73,1,8173,477,432;74,1,8174,472,432;75,1,8175,468,431;76,1,8176,464,431;77,1,8177,458,430;78,1,8178,454,430;79,1,8179,450,429;80,1,8180,447,428;81,1,8181,444,428;82,1,8182,441,428;83,1,8183,437,427;84,1,8184,434,427;85,1,8185,430,426;86,1,8186,428,426;87,1,8187,424,426;88,1,8188,422,426;89,1,8189,420,425;90,1,8190,417,425;91,1,8191,414,424;92,1,8192,412,424;93,1,8193,410,424;94,1,8194,408,424;95,1,8195,406,423;96,1,8196,404,423;97,1,8197,401,423;98,1,8198,400,422;99,1,8199,398,422;369,3,10402,684,393,-1;-1,2,-94,-117,-1,2,-94,-111,0,387,-1,-1,-1;-1,2,-94,-109,0,384,-1,-1,-1,-1,-1,-1,-1,-1,-1;-1,2,-94,-114,-1,2,-94,-103,2,940;3,10401;-1,2,-94,-112,https://s.activision.com/activision/announcement2FA-1,2,-94,-115,1,946307,32,387,384,0,947046,10402,0,1606229358950,9,17182,0,370,2863,1,0,10403,825910,0,7EF4795B893873F658669419D047D0BD~-1~YAAQL+F6XNkVm4h1AQAAmPe6+gT1QLZTGPHiT74bAFh7ogk1hBV3F86qPj9QQxaM8L0g28dhP7u3HiNTm3lOWqu+hqfws4LCG2fGoT/+6MqDnrh2WFH1akj6G1xz40uczzTQAhNB7aP5BeV+2ZST3MZXjkjT2K1JJp4oPZihnw9CoBHWldduZikV3sp4wfK8Cjt5ObHCFxk7bNIO5LJJlgaqE6ZoafESKf6bC83CcoghejplBKfdjEjIgLstzzmfSCZMpXA8sHFhlTaa0hCtPVdsTIWZd71nlWzA1OSJCECYJfGfVZWtwgJ9t+Xm05zJO6zNzNsyaLi+eebd9z2z5WsN1F1LnhMmLM6N~-1~-1~-1,32854,20,425232752,30261693,PiZtE,87905,32-1,2,-94,-106,1,2-1,2,-94,-119,37,40,41,39,48,46,9,6,5,4,4,4,7,1758,-1,2,-94,-122,0,0,0,0,1,0,0-1,2,-94,-123,-1,2,-94,-124,-1,2,-94,-126,-1,2,-94,-127,11321144041300043122-1,2,-94,-70,311072444;248864222;dis;;true;true;true;-240;true;24;24;true;false;-1-1,2,-94,-80,5271-1,2,-94,-116,1263458673-1,2,-94,-118,181828-1,2,-94,-129,f9a811b8b59b3105f042b25a8eb024b1d3a535fcd10249efb20ffcfafdb6dca6,1.5,7c535402e0cd5007e5c52235cf614eef4ac0185d6ea016852e12d8dc73a502ca,,,,0-1,2,-94,-121,;3;6;0\"}");
                var req = await Client.SendAsyncRedirect(reqMsg);
                var res = await req.Content.ReadAsStringAsync();
                return new ActivisionResponse(res.Contains("true"));
            }

        }

        public async Task<ActivisionResponse> Login(int maxRetries = 3) {
            string jsonInput = $"{{\"deviceId\":\"{userAgent}\"}}";
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, new Uri("https://profile.callofduty.com/cod/mapp/registerDevice"))) {
                using (var content = new StringContent(jsonInput, Encoding.UTF8, "application/json")) {
                    reqMsg.Headers.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64; Trident/7.0; rv:11.0) like Gecko");
                    reqMsg.Headers.TryAddWithoutValidation("Pragma", "no-cache");
                    reqMsg.Headers.TryAddWithoutValidation("Accept", "application/json");
                    reqMsg.Headers.TryAddWithoutValidation("Content-Type", "application/json");
                    reqMsg.Content = content;
                    var req = await Client.SendAsync(reqMsg);
                    if (!req.IsSuccessStatusCode)
                        return new ActivisionResponse(false);
                    var res = await req.Content.ReadAsStringAsync();
                    var _json = JObject.Parse(res);
                    authHeader = (string)_json["data"]["authHeader"];
                }
            }
            string payload = $"{{\"email\":\"{account.Email}\",\"password\":\"{account.Password}\"}}";

            logger.Debug("Logging into auth acc " + this.ToString());
            var uri = new Uri("https://profile.callofduty.com/cod/mapp/login");
            using (var reqMsg = new HttpRequestMessage(HttpMethod.Post, uri)) {
                using (var content = new StringContent(payload, Encoding.UTF8, "application/json")) {
                    reqMsg.Headers.TryAddWithoutValidation("Accept", "application/json");
                    Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", "bearer " + authHeader);
                    reqMsg.Content = content;
                    logger.Debug($"Sent request to get cookies of {account}");
                    var res = string.Empty;
                    var req = await this.Client.SendAsync(reqMsg);
                    if (req.IsSuccessStatusCode) {
                        res = await req.Content.ReadAsStringAsync();
                        var loggedIn = !res.Contains(",\"success\":false}");
                        if (!loggedIn) new ActivisionResponse(false, res);
                        //add act_sso_cookie
                        var json = JObject.Parse(res);
                        string actSso = (string)json["ACT_SSO_COOKIE"], atkn = (string)json["atkn"], rtkn = (string)json["rtkn"];
                        if (string.IsNullOrEmpty(actSso) || string.IsNullOrEmpty(atkn)) return new ActivisionResponse(false, res);
                        this.Client.DefaultRequestHeaders.TryAddWithoutValidation("atvi-auth", $"Bearer {actSso}");
                        this.CookiesString = $"ACT_SSO_COOKIE={actSso};atkn={atkn};";
                        Cookies.Add(new Uri("https://s.activision.com/activision/profile"), new Cookie("ACT_SSO_COOKIE", actSso));
                        Cookies.Add(new Uri("https://s.activision.com/activision/profile"), new Cookie("atkn", atkn));
                        Cookies.Add(new Uri("https://s.activision.com/activision/profile"), new Cookie("rtkn", rtkn));
                        //this.Client.DefaultRequestHeaders.TryAddWithoutValidation("cookie", $"ACT_SSO_COOKIE={actSso};atkn={atkn};");
                        return new ActivisionResponse(true, res);
                    }
                    return new ActivisionResponse(false);
                }
            }
        }

        public async Task<ActivisionResponse> GetCSRF(int maxRetries = 3) {
            int retry = 0;
            while (retry++ < maxRetries) {
                logger.Debug($"Getting CSRF token at try #{retry}");
                var req = await this.Client.GetAsync("cod/login");
                if (req.IsSuccessStatusCode) {
                    Cookie xsrf = this.Cookies.GetCookies(this.Client.BaseAddress)["XSRF-TOKEN"];
                    if (xsrf == null) {
                        logger.Error($"Couldn't get CSRF token at retry #{retry}.... retrying");
                        logger.Error(await req.Content.ReadAsStringAsync());
                        continue;
                    } else {
                        logger.Debug("Got XSRF-TOKEN with the value of: " + xsrf.Value);
                        return new ActivisionResponse(true, xsrf.Value);
                    }
                } else return new ActivisionResponse(false, "Could not get CSRF token");
            }
            throw new TimeoutException($"Failed to get CSRF.... Reached maximum tries of {maxRetries}");
        }
    }
}
