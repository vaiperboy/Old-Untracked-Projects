using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Console = Colorful.Console;
using Flurl.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using System.Net;
using System.IO;
using System.Net.Http.Headers;
using Newtonsoft.Json;
using System.Threading;
using HtmlAgilityPack;
using System.Linq;

namespace Discord_Bots {
    public static class Commands {
        
        public static async Task<HttpResponseMessage> VoteForBot(this Token token, string client_id) {
            if (!string.IsNullOrEmpty(token.Outh))
                token.Outh = await RegisterOuth(token);
            if (!string.IsNullOrEmpty(token.Outh)) {
                var csrf = await Helper.GetCsrf(token.Outh);
                if (!string.IsNullOrEmpty(csrf)) {
                    var jsonIn = new JObject();
                    jsonIn.Add("bot", client_id);
                    jsonIn.Add("type", "upvote");
                    jsonIn.Add("query", "");
                    jsonIn.Add("csrf", csrf);

                    var req = await "https://discordbots.org/api/vote"
                        .WithHeader("Content-Type", "application/json")
                        .WithHeader("cookie", token.Outh)
                        .AllowAnyHttpStatus()
                        .PostJsonAsync(jsonIn);
                    return req;
                }
            }
            return new HttpResponseMessage();
        }
        public static async Task<string> RegisterOuth(this Token token) {

            /*
             * {
                    "permissions": 0,
                    "authorize": true
                }*/
            var json = new JObject();
            json.Add("permissions", 0);
            json.Add("authorize", true);
            var uri = "https://discordapp.com/api/v6/oauth2/authorize?" +
                $"client_id=264434993625956352" +
                "&redirect_uri=https://discordbots.org/login/callback" +
                "&response_type=code" +
                "&scope=identify" +
                "&state=L2JvdC8zNjU5NzU2NTU2MDg3NDU5ODUvdm90ZQ==&prompt=consent";
            var req = await uri
                .WithHeader("authorization", token.token)
                .WithHeader("content-type", "application/json")
                .AllowAnyHttpStatus()
                .PostJsonAsync(json);
            var res = await req.Content.ReadAsStringAsync();
            req.Dispose();
            var location = (string)(JObject.Parse(res))["location"];
            if (location.Contains("error"))
                return string.Empty;
            using (HttpClient client = new HttpClient()) {
                var _req = await client.GetAsync(location);
                if (!string.IsNullOrEmpty(_req.GetHeaderValue("set-cookie"))) {
                    return _req.GetHeaderValue("set-cookie").Split(';')[0];
                }
            }
            return string.Empty;
                //return _req.GetHeaderValue("set-cookie").GetBetween("connect.sid=", ";");
        }

        public static async Task<Tuple<string, string>> CreateServer(this Token token) {

            //{"name":"tst 44","region":"russia","icon":null}
            var jsonInput = new JObject();
            var rand = new Random();
            jsonInput.Add("name", $"best {rand.Next(0, 1000)}");
            jsonInput.Add("region", "russia");
            jsonInput.Add("icon", null);

            var req = await $"https://discordapp.com/api/v6/guilds"
            .WithHeader("accept", "*/*")
            .WithHeader("accept-encoding", "gzip, deflate, br")
            .WithHeader("accept-language", "en-US")
            .WithHeader("origin", " https://discordapp.com")
            .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36").WithHeader("authorization", token)
            .WithHeader("authorization", token.token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(jsonInput);

            if ((int)req.StatusCode == 201
                || req.StatusCode == HttpStatusCode.OK) {
                var res = await req.Content.ReadAsStringAsync();
                var jsonOut = JObject.Parse(res);
                string serverInv = string.Empty,
                    channelId = string.Empty,
                    serverId = string.Empty;
                if (!string.IsNullOrEmpty((string)jsonOut["system_channel_id"])) {
                    serverInv =  await token.InviteServer((string)jsonOut["system_channel_id"]);
                    channelId = (string)jsonOut["system_channel_id"];
                }
                if (!string.IsNullOrEmpty((string)jsonOut["id"])) {
                    serverId = (string)jsonOut["id"];
                }
                return Tuple.Create(serverId, serverInv);
            }

            return Tuple.Create(string.Empty, string.Empty);
        }

        public static async Task<string> InviteServer(this Token token, string id) {

            //{"validate":"tVvFTt","max_age":86400,"max_uses":0,"target_user_type":null,"temporary":false}
            var jsonInput = new JObject();
            jsonInput.Add("max_age", 86400);
            jsonInput.Add("max_uses", 0);
            jsonInput.Add("target_user_type", null);
            jsonInput.Add("temporary", false);

            var req = await $"https://discordapp.com/api/v6/channels/{id}/invites"
            .WithHeader("accept", "*/*")
            .WithHeader("accept-encoding", "gzip, deflate, br")
            .WithHeader("accept-language", "en-US")
            .WithHeader("origin", " https://discordapp.com")
            .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36").WithHeader("authorization", token)
            .WithHeader("authorization", token.token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(jsonInput);

            if ((int)req.StatusCode == 201
                || req.StatusCode == HttpStatusCode.OK) {
                var res = await req.Content.ReadAsStringAsync();
                var jsonOut = JObject.Parse(res);
                if (!string.IsNullOrEmpty((string)jsonOut["code"])) {
                    return (string)jsonOut["code"];
                }
            }

            return string.Empty;
        }


        public static async Task<HttpResponseMessage> JoinServer(this Token token, string code) {
            var req = await $"https://discordapp.com/api/v6/invites/{code}"
            .WithHeader("accept", "*/*")
            .WithHeader("accept-encoding", "gzip, deflate, br")
            .WithHeader("accept-language", "en-US")
            .WithHeader("origin", " https://discordapp.com")
            .WithHeader("content-length", "0")
            .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36").WithHeader("authorization", token)
            .WithHeader("authorization", token.token)
            .AllowAnyHttpStatus()
            .PostStringAsync("");

            return req;
        }

        public static async Task<string> CreateChannel(this Token token, string id, string name = "claim-1") {


            //{"type":0,"name":"claim","permission_overwrites":[]}
            var jsonInput = new JObject();
            var a = new string[0];
            jsonInput.Add("type", 0);
            jsonInput.Add("name", name);
            //jsonInput.Add("permission_overwrites", JsonConvert.SerializeObject(new List<int> { }));


            var req = await $"https://discordapp.com/api/v6/guilds/{id}/channels"
            .WithHeader("content-type", "application/json")
            .WithHeader("authorization", token.token)
            .AllowAnyHttpStatus()
            .PostJsonAsync(jsonInput);

            var res = await req.Content.ReadAsStringAsync();
            if ((int)req.StatusCode == 201
                || req.StatusCode == HttpStatusCode.OK) {
                res = await req.Content.ReadAsStringAsync();
                var jsonOut = JObject.Parse(res);
                if (!string.IsNullOrEmpty((string)jsonOut["id"])) {
                    return (string)jsonOut["id"];
                }
            }

            return string.Empty;
        }

        public static async Task<HttpResponseMessage> SendChannelMessage(this Token token, string channel, string message, int delay = 0) {
            var rand = new Random();
            var jsonInput = new JObject();
            jsonInput.Add("content", message);
            jsonInput.Add("tts", "false");
            var req = await $"https://discordapp.com/api/v6/channels/{channel}/messages"
                .WithHeader("accept", "*/*")
                .WithHeader("accept-encoding", "gzip, deflate, br")
                .WithHeader("accept-language", "en-US")
                .WithHeader("content-type", "application/json")
                .WithHeader("origin", " https://discordapp.com")
                .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36")
                .WithHeader("authorization", token.token)
                .AllowAnyHttpStatus()
                .PostJsonAsync(jsonInput);

            var res = await req.Content.ReadAsStringAsync();
            var jsonOut = JObject.Parse(res);
            await Task.Delay(delay);
            //return req.StatusCode == System.Net.HttpStatusCode.OK;
            return req;
        }

        public static async Task<string> SendChannelMessage_TEST(string token, string channel, string message, int delay = 0) {
            var rand = new Random();
            var jsonInput = new JObject();
            jsonInput.Add("content", message);
            jsonInput.Add("tts", "false");
            var req = await $"https://discordapp.com/api/v6/channels/{channel}/messages"
                .WithHeader("accept", "*/*")
                .WithHeader("accept-encoding", "gzip, deflate, br")
                .WithHeader("accept-language", "en-US")
                .WithHeader("content-type", "application/json")
                .WithHeader("cookie", " __cfduid=d4e8bd926597007e68ffafed99a2398a11554234862; __cfruid=6465f5b19c56ae6c072e982e2a3df5eaef93595f-1560495219")
                .WithHeader("origin", " https://discordapp.com")
                .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36")
                .WithHeader("authorization", token)
                .AllowAnyHttpStatus()
                .PostJsonAsync(jsonInput);

            var res = await req.Content.ReadAsStringAsync();
            var jsonOut = JObject.Parse(res);
            await Task.Delay(delay);
            req.Dispose();
            //return req.StatusCode == System.Net.HttpStatusCode.OK;
            return res;
        }

        public static Task<JObject> sendDirectMessage(this Token token, string idTarget, string message) {
            throw new NotImplementedException();
            ////594823549211115521
            //var rand = new Random(DateTime.Now.Second);
            //var jsonInput = new JObject();
            ////jsonInput.Add("recipients", new List<string>() { idTarget });
            //jsonInput = JObject.Parse($"{{\"recipients\":[\"{idTarget}\"]}}");
            //var req = await $"https://discordapp.com/api/v6/users/123/channels"
            //    .WithHeader("accept", "*/*")
            //    .WithHeader("accept-encoding", "gzip, deflate, br")
            //    .WithHeader("accept-language", "en-US")
            //    .WithHeader("content-type", "application/json")
            //    .WithHeader("origin", " https://discordapp.com")
            //    .WithHeader("Referer", "https://discordapp.com/channels/588999508298563604/588999508298563606")
            //    .WithHeader("user-agent", " Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) discord/0.0.305 Chrome/69.0.3497.128 Electron/4.0.8 Safari/537.36")
            //    .WithHeader("authorization", token.token)
            //    .AllowAnyHttpStatus()
            //    .PostJsonAsync(jsonInput);
            //var res = await req.Content.ReadAsStringAsync();
            //var resJson = JObject.Parse(res);

            //return resJson;
        }
    }
}
