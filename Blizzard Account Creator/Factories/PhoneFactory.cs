using NLog;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Blizzard_Account_Creator {
    public enum PhoneProviders {
        getsms,
        smsActivate,
        _5sim
    }
    class PhoneFactory {
        public string Key { get; set; }
        public double Balance { get; private set; }
        private double PricePerUnit { get; set; }
        public int MaxUnits { get {
                return (int)Math.Floor(((Balance * .95) / PricePerUnit));
            } }
        private readonly HttpClient Client = new HttpClient();
        public PhoneProviders Provider { get; }
        public int MaxRequestsPerSecond { get; set; }
        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        private ConcurrentStack<DateTime> Stack = new ConcurrentStack<DateTime>();
        private static Dictionary<PhoneProviders, Uri> ProvidersList = new Dictionary<PhoneProviders, Uri> {
            {PhoneProviders.getsms,  new System.Uri("http://api.getsms.online/stubs/")},
            {PhoneProviders.smsActivate,  new System.Uri("https://sms-activate.ru/stubs/")},
            {PhoneProviders._5sim,  new System.Uri("http://api1.5sim.net/stubs/")},
        };

        public PhoneFactory(string key, PhoneProviders provider, bool getBalance = false, int maxReqs = 10) {
            this.Provider = provider;
            this.Key = key;
            this.Provider = provider;
            this.Client.BaseAddress = ProvidersList[this.Provider];
            this.MaxRequestsPerSecond = maxReqs;

            if (getBalance) {
                var req = GetAsync($"handler_api.php?api_key={this.Key}&action=getBalance").Result;
                var res = req.Content.ReadAsStringAsync().Result;
                if (req.IsSuccessStatusCode) {
                    var split = res.Split(":");
                    if (split.Length > 1 && double.TryParse(split[1], out double _balance)) {
                        this.Balance = _balance;
                    } else this.Balance = -1;
                } else this.Balance = -1;
            }
        }

        public async Task<Responses.GeneratePhoneResponse> GetPhoneNumber(string country = "kz") {
            HttpResponseMessage req = null;
            switch (this.Provider) {
                case PhoneProviders.getsms:
                    req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getNumber&service=ot&country={country}");
                    break;
                case PhoneProviders.smsActivate:
                    req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getNumber&service=bz&country={(country.ToLower().StartsWith('k') ? "0" : "39")}");
                    break;
                case PhoneProviders._5sim:
                    country = (country.ToLower().StartsWith('k') ? "ru" : country);
                    //if (country.Equals("ru")) 
                    //    req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getNumber&service=blizzard&country={country}&operator=virtual27");
                    // else 
                        req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getNumber&service=blizzard&country={country}");
                    break;
            }

            var res = await req.Content.ReadAsStringAsync();
            if (req.IsSuccessStatusCode) {
                var split = res.Split(":");
                if (split.Length > 2) {
                    return new Responses.GeneratePhoneResponse(true, split[2], split[1]);
                }
            }
            return Responses.GeneratePhoneResponse.Empty;
        }
        
        public async Task<Responses.ActivationCodeResponse> GetActivationCode(Responses.GeneratePhoneResponse phone,
            TimeSpan timeOut = default) {
            if (timeOut == default) timeOut = TimeSpan.FromMinutes(3);
            if (this.Provider == PhoneProviders.getsms || this.Provider == PhoneProviders._5sim) {
                var setCode = await SetActivationCode(phone, "1");
                if (setCode.isSuccess) {
                    var delay = TimeSpan.FromSeconds(10);
                    int counter = 1;
                    string response;
                    do {
                        await Task.Delay(delay);
                        var req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getStatus&id={phone.OrderID}");
                        response = await req.Content.ReadAsStringAsync();
                        if (response.Contains("status_error", StringComparison.OrdinalIgnoreCase))
                            return new Responses.ActivationCodeResponse(false, response);
                        if ((counter++ * delay.TotalSeconds) >= timeOut.TotalSeconds) {
                            await this.SetActivationCode(phone, "-1");
                            return new Responses.ActivationCodeResponse(false, "Timeout..didn't receive code", true);
                        }
                    } while (!response.Contains("status_ok", StringComparison.OrdinalIgnoreCase));
                    //response received?
                    var split = response.Split(":");
                    await this.SetActivationCode(phone, "6");
                    return new Responses.ActivationCodeResponse(true, split[^1].ExtractCode(), response);
                }
            } else if (this.Provider == PhoneProviders.smsActivate) {
                var setCode = await SetActivationCode(phone, "1");
                if (setCode.isSuccess) {
                    var delay = TimeSpan.FromSeconds(10);
                    int counter = 1;
                    string response;
                    do {
                        await Task.Delay(delay);
                        var req = await GetAsync($"handler_api.php?api_key={this.Key}&action=getStatus&id={phone.OrderID}");
                        response = await req.Content.ReadAsStringAsync();
                        if (response.Contains("status_error", StringComparison.OrdinalIgnoreCase))
                            return new Responses.ActivationCodeResponse(false, response);
                        if ((counter++ * delay.TotalSeconds) >= timeOut.TotalSeconds) {
                            await DisposeNumber(phone);
                            return new Responses.ActivationCodeResponse(false, "Timeout..didn't receive code", true);
                        }
                    } while (!response.Contains("status_ok", StringComparison.OrdinalIgnoreCase));
                    //response received?
                    var split = response.Split(":");
                    await this.SetActivationCode(phone, "6");
                    return new Responses.ActivationCodeResponse(true, split[^1].ExtractCode(), response);
                }
            }
            return Responses.ActivationCodeResponse.Empty;
        }

        public async Task<Responses.SetActivationCodeResponse> DisposeNumber(Responses.GeneratePhoneResponse phone) {
            return await this.SetActivationCode(phone, "-1");
        }

        private async Task<Responses.SetActivationCodeResponse> SetActivationCode(Responses.GeneratePhoneResponse phone, string code) {
            var req = await GetAsync($"handler_api.php?api_key={this.Key}&action=setStatus&id={phone.OrderID}&status={code}");
            var res = await req.Content.ReadAsStringAsync();
            if (req.IsSuccessStatusCode) {
                return new Responses.SetActivationCodeResponse(res.Equals("access_ready", StringComparison.OrdinalIgnoreCase), res);
            }
            return Responses.SetActivationCodeResponse.Empty;
        }

        private async Task<HttpResponseMessage> GetAsync(string url) {
            while (Helper.GetDateTimes(this.Stack, TimeSpan.FromSeconds(1)).Length >= this.MaxRequestsPerSecond) {
                
                await Task.Delay(1000);
            }
            Stack.Push(DateTime.Now);
            return await Client.GetAsync(url);
        }

       
    }
}
