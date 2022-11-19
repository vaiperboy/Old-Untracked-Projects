using COD_Discord_Bot.Exceptions;
using COD_Discord_Bot.Responses;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Crypto.Signers;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Shoppy {
     class Shoppy {
        public string API_Key { get; private set; }
        private HttpClient Client;
        private ShoppyConfig Config;
        private static List<ShoppyOrderResponse> cache = new List<ShoppyOrderResponse>();
        
        public Shoppy(string key, ShoppyConfig config) {
            this.Config = config;
            this.API_Key = key;
            using (var tcp = new TcpClient()) {
                try {
                    tcp.Connect("127.0.0.1", 8888);
                    Client = new HttpClient(new HttpClientHandler() {
                        Proxy = new WebProxy("127.0.0.1:8888")
                    }) {
                        BaseAddress = new Uri("https://shoppy.gg/api/v1/")
                    };
                } catch (Exception) {
                    Client = new HttpClient {
                        BaseAddress = new Uri("https://shoppy.gg/api/v1/")
                    };
                }
            }
            
           
            Client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64)" +
                " AppleWebKit/537.36 (KHTML, like Gecko) Chrome/85.0.4183.121 Safari/537.36");
            Client.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", API_Key);
            HttpResponseMessage req = Client.GetAsync("orders/").Result;
            if (req.StatusCode.Equals(HttpStatusCode.Unauthorized))
                throw new ShoppyException("Invalid Shoppy API key!!!");
        }

        public async Task<bool> DeleteProduct(string productId) {
            var req = await Client.DeleteAsync($"products/{productId}");
            return req.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateProduct(string productId, JObject newProduct) {
            var title = (string)newProduct["title"];
            if (title.Length < 3) {
                title = title + new string('-', 3 - title.Length);
                newProduct["title"] = title;
            }
            var content = new StringContent(newProduct.ToString(), Encoding.UTF8, "application/json");
            var req = await this.Client.PostAsync($"products/{productId}", content);
            if (req.IsSuccessStatusCode) {
                var res = await req.Content.ReadAsStringAsync();
                var json = JObject.Parse(res);
                return (bool)json["status"];
            }
            return false;
        }

        public async Task<ShoppyProduct> DuplicateProduct(string productId = default) {
            if (productId == default)
                productId = Config.ShoppyProductToDuplicate;
            var req = await Client.PostAsync($"products/{productId}/duplicate", null);
            if (req.IsSuccessStatusCode) {
                var res = await req.Content.ReadAsStringAsync();
                var json = JObject.Parse(res);
                if (bool.Parse((string)json["status"])) {
                    return new ShoppyProduct { JSON = (JObject)json["resource"] };
                } else return default;
            } else return default;
        }
        
        public async Task<ShoppyUser> LocateOrderIDs(string targetEmail, int maxPage = 30, DiscordMessage msg = default) {
            var user = new ShoppyUser();
            for (int i = 1; i <= maxPage; i++) {
                using (var req = await Client.GetAsync($"orders?page={i}")) {
                    if (!req.IsSuccessStatusCode)
                        return new ShoppyUser("Couldn't fetch order page!!");
                    var json = JArray.Parse(await req.Content.ReadAsStringAsync());
                    if (json.Count == 0) break;
                    for (int j = 0; j < json.Count; j++) {
                        var email = (string)json[j]["email"];
                        if (email.Equals(targetEmail, StringComparison.OrdinalIgnoreCase)) {
                            user.Email = email;
                            user.OrderIDs.Add((string)json[j]["id"]);
                        }
                    }
                    if (msg != default) {
                        if (i % Config.UpdateRate == 0 || i + 1 > maxPage)
                            await msg.ModifyAsync($"**At page {i}/{maxPage}... _(update frequency: {Config.UpdateRate})_**");
                    }
                    await Task.Delay(Config.Delays);
                }
            }
            return user;
        }

        public async Task<ShoppyOrderResponse> GetOrder(string orderId) {
            if (!orderId.IsValidOrderId())
                throw new ShoppyException($"{orderId} is invalid!");
            var cacheOrder = cache
                .Where(x => x.OrderID.Equals(orderId, StringComparison.OrdinalIgnoreCase))
                .FirstOrDefault();
            if (cacheOrder != default)
                return cacheOrder;
            var req = await Client.GetAsync($"orders/{orderId}");
            if (req.StatusCode == HttpStatusCode.Forbidden)
                return new ShoppyOrderResponse(false, "This order ID is not for my shop!");
            var res = await req.Content.ReadAsStringAsync();

            if (req.IsSuccessStatusCode) {
                try {
                    var json = JObject.Parse(res);
                    if ((string)json["paid_at"] == null) {
                        return new ShoppyOrderResponse(false, "User didn't complete payment??\nOr crypto not confirmed yet...");
                    }
                    var accounts = ((JArray)json["accounts"]).ParseAccounts();
                    var dateBought = DateTime.Parse((string)json["paid_at"]);
                    var order = new ShoppyOrderResponse(true, accounts) {
                        Email = (string)json["email"],
                        ItemName = (string)json["product"]["title"],
                        PricePaid = (double)json["price"],
                        DateBought = dateBought,
                        IsCustom = accounts.Count == 0 
                    };
                    var obj = (JToken)json["custom_fields"];
                    if (obj.HasValues) {
                        var custom_fields = (JArray)json["custom_fields"];
                        if (custom_fields != default && custom_fields.Count > 0) {
                            //assume first field is discord id
                            foreach (var field in custom_fields) {
                                if (((string)field["name"]).Contains("discord id", StringComparison.OrdinalIgnoreCase)) {
                                    order.DiscordID = (ulong)field["value"];
                                    break;
                                }
                            }
                        }
                    }

                    return order;
                } catch (Exception) {
                    return new ShoppyOrderResponse(false, "Couldn't parse JSON");
                }
            } else if (res.Contains("false")) return new ShoppyOrderResponse(false, "No order with that ID found???");
            else return new ShoppyOrderResponse(false, res);
        }
    }
}
