using COD_Discord_Bot.Classes;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Text;

namespace COD_Discord_Bot.Shoppy {
    class ShoppyProduct {
        public JObject JSON { get; set; }
        private ShoppySampleProduct sampleProduct;
        public ShoppySampleProduct SampleProduct { get {
                return this.sampleProduct;
            } set {
                this.sampleProduct = value;
                if (this.JSON != default) {
                    JSON["unlisted"] = false;
                    JSON["title"] = sampleProduct.Title;
                    JSON["price"] = (float)Math.Round(sampleProduct.Price, 2);
                    JSON["accounts"] = JArray.FromObject(sampleProduct.Items);
                    JSON["stock"] = sampleProduct.Items.Count;
                    JSON.Add("model", "Product");
                }
            }
        }

        public override string ToString() {
            return JSON == default ? string.Empty : $"https://shoppy.gg/product/{JSON["id"]}";
        }
    }
}
