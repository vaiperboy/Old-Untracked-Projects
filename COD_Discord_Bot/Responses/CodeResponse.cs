using COD_Discord_Bot.Classes;
using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Responses {
    public interface IResponse {
        public bool Success { get; set; }
        public string Message { get; set; }
    }

    public class ActivisionResponse : IResponse {
        public bool Success { get; set; }
        public string Message { get; set; }

        public ActivisionResponse(bool success, string msg = "") {
            this.Success = success;
            this.Message = msg;
        }

        public override string ToString() {
            return Message;
        }
    }

    class CodeResponse : IResponse {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<Code> Codes { get; private set; }

        public CodeResponse(bool success, string msg, List<Code> codes) {
            this.Success = success;
            this.Message = msg;
            this.Codes = codes;
        }

        public CodeResponse(bool success, string msg) {
            this.Success = success;
            this.Message = msg;
        }
    }

    public class ShoppyResponse : IResponse {
        public bool Success { get; set; }
        public string Message { get; set; }
        public static ShoppyResponse Empty = new ShoppyResponse();
        public ShoppyResponse() {

        }

        public ShoppyResponse(bool success, string message) {
            this.Success = success;
            this.Message = message;
        }
    }

    public class ShoppyUser : ShoppyResponse {
        public string Email { get; set; }
        public List<string> OrderIDs { get; set; } = new List<string>();
        public bool Success { get {
                return OrderIDs.Count > 0;
            } }
        public static new ShoppyUser Empty = new ShoppyUser();
        public ShoppyUser() {

        }

        public ShoppyUser(List<string> orderids) {
            this.OrderIDs = orderids;
        }

        public ShoppyUser(string msg) {
            this.Message = msg;
        }

        public override string ToString() {
            if (!Success) return Message;
            var msg = $"Found {OrderIDs.Count} order for {Email}:\n";
            for (int i = 0; i < OrderIDs.Count; i++) {
                msg += OrderIDs[i];
                if (i + 1 < OrderIDs.Count) msg += "\n";
            }
            return msg;
        }
    }

    public class ShoppyOrderResponse : ShoppyResponse {
        public List<string> Products { get; set; }
        public int BoughtCount { get {
                //if custom payment consider 1 for multiplier
                return IsCustom ? 1 : Products.Count;
            } }
        public string Email { get; set; }
        public string OrderID { get; set; }
        public string ItemName { get; set; }
        public double PricePaid { get; set; }

        public ulong DiscordID { get; set; }
        public DateTime DateBought { get; set; }

        public TimeSpan WhenBought { get {
                return (DateTime.Now - DateBought);
            } }
        public bool IsCustom { get; set; } = false;

        public static ShoppyOrderResponse Empty = new ShoppyOrderResponse();

        public ShoppyOrderResponse(bool success, List<string> products) {
            this.Success = success;
            this.Products = products;
        }

        public ShoppyOrderResponse(bool success, string msg) {
            this.Message = msg;
            this.Success = success;
        }

        private ShoppyOrderResponse() {

        }

        public override string ToString() {
            if (Success) {
                var msg = $"**Email:** {Email}\n" +
                    $"**Item Bought:** _**{BoughtCount}**_ x {ItemName}\n" +
                    $"**Price Paid:** {((double)(PricePaid * BoughtCount))}$";
                if (DateBought != default) {
                    var date = DateBought.ToString("MM/dd/yyyy H:mm");
                    msg += $"\n**Bought on**: {date} - ({WhenBought.FormatTimeSpan()})";
                }
                return msg;
            } else return Message;
        }
    }
}
