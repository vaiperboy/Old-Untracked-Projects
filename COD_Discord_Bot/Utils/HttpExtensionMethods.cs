using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Utils {
    public static class HttpExtensionMethods {
        public static async Task<HttpResponseMessage> SendAsyncRedirect(this HttpClient client, HttpRequestMessage requestMsg) {
            var req = await client.SendAsync(requestMsg);
            if (req.StatusCode == (HttpStatusCode)302) {
                if (req.Headers.TryGetValues("Location", out var locations)) {
                    req.RequestMessage.RequestUri = new Uri(locations.First());
                } 
            }
            return req;
        }
    }
}
