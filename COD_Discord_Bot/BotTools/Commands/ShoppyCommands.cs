using COD_Discord_Bot.Classes.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using NLog;
using System.Threading.Tasks;
using COD_Discord_Bot.Utils;
using COD_Discord_Bot.Classes;
using System.Collections.Generic;

namespace COD_Discord_Bot {
    public partial class ShoppyCommands : DSharpPlus.CommandsNext.BaseCommandModule {
        
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private GSheet gSheet = new GSheet();
        public ShoppyCommands() {

        }

        private bool TryGetShoppy(CommandContext ctx, out Shoppy.Shoppy shoppy, bool requiresServer = true) {
            shoppy = default;
            if (ctx.Guild == default) {
                ctx.RespondAsync("This command has to be done in a server you retard...").Wait();
                return false;
            }
            if (Configuration.Shoppys.TryGetValue(ctx.Guild.Id, out shoppy)) {
                return true;
            }
            //else
            ctx.RespondAsync("No shoppy configured for this server??").Wait();
            return false;
        }
        [Command("order")]
        [RequireShoppyPermissions]
        public async Task Order(CommandContext ctx, [RemainingText] string msg) {
            if (TryGetShoppy(ctx, out Shoppy.Shoppy shoppy)) {
                if (!msg.TryParseOrderId(out string orderId)) {
                    await ctx.RespondAsync("Invalid Order ID format!!");
                    return;
                }
                var accounts = await shoppy.GetOrder(orderId);
                if (accounts.Success) {
                    await ctx.RespondAsync(embed: DiscordHelper.CreateEmbed("OrderHelper", accounts.ToString()));
                    if (accounts.Products.Count > 0)
                        await ctx.RespondWithFileAsync("account.txt", string.Join('\n', accounts.Products).AsStream());
                } else {
                    await ctx.RespondAsync(embed: DiscordHelper.CreateEmbed("OrderHelper", accounts.ToString(), true));
                }
            }
            
        }

        [Command("locate")]
        [RequireShoppyPermissions]
        public async Task Locate(CommandContext ctx, string email, int pages = 30) {
            if (TryGetShoppy(ctx, out Shoppy.Shoppy shoppy)) {
                if (!email.IsValidEmail()) {
                    await ctx.RespondAsync("invalid email... retard");
                    return;
                }
                var msg = await ctx.RespondAsync("Finding it...");
                await ctx.TriggerTypingAsync();
                var orderIds = await shoppy.LocateOrderIDs(email, pages, msg);
                var embed = DiscordHelper.CreateEmbed("OrderHelper", orderIds.ToString(), !orderIds.Success);
                await msg.ModifyAsync(embed: embed);
            }
        }

        
        [Command("deleteShoppy")]
        [RequireShoppyPermissions]
        public async Task DeleteShoppy(CommandContext ctx, [RemainingText] string links) {
            if (TryGetShoppy(ctx, out Shoppy.Shoppy shoppy)) {
                if (links.Split(" ").TryParseProductId(out List<string> orderIds)) {
                    int deleted = 0;
                    foreach (var product in orderIds) {
                        var req = await shoppy.DeleteProduct(product);
                        if (req)
                            deleted++;
                    }
                    if (deleted == 0) await ctx.RespondAsync("Couldn't delete product :/");
                    else await ctx.RespondAsync($"Deleted {deleted} product{(deleted == 1 ? "" : "s")}");
                }
            }
        }

        [Command("makeShoppy")]
        [RequireShoppyPermissions]
        public async Task MakeShoppy(CommandContext ctx, string hName, float price = default, [RemainingText] string title = default) {
            if (TryGetShoppy(ctx, out Shoppy.Shoppy shoppy)) {
                ShoppySampleProduct product = new ShoppySampleProduct();
                if (hName.Equals("custom", System.StringComparison.OrdinalIgnoreCase)) {
                    if (price == default) {
                        await ctx.RespondAsync("At least put a price idiot?");
                        return;
                    }
                    product = new ShoppySampleProduct() {
                        Items = { "TAKE PICTURE OF THIS RECEIPT" },
                        Price = price,
                        Title = title ?? "Custom payment"
                    };
                } else if (hName.TryParseEmails(out List<string> emails)) {
                    if (price == default) {
                        await ctx.RespondAsync("At least put a price idiot?");
                        return;
                    }
                    product = new ShoppySampleProduct() {
                        Items = emails,
                        Price = price,
                        Title = title ?? "Custom payment"
                    };
                } else {
                    if (ctx.Guild.Id != (ulong)ServerIds.CodAccounts) {
                        await ctx.RespondAsync("You can't use me....");
                        return;
                    }
                    product = await gSheet.GetProduct(hName);
                    if (string.IsNullOrEmpty(product.Title) || product.Price == default || product.Items.Count == 0) {
                        await ctx.RespondAsync("Not all fields are correct in the sheet... make sure price, title and account(s) are there you fucking tard");
                        return;
                    }
                    if (product == default) {
                        await ctx.RespondAsync("Couldn't get account from sheets :/");
                        return;
                    }
                }

                var originalOrder = await shoppy.DuplicateProduct();
                if (originalOrder == default) {
                    await ctx.RespondAsync("Couldn't duplicate original order :/");
                    return;
                }
                originalOrder.SampleProduct = product;
                if (title != default)
                    originalOrder.SampleProduct.Title = title;
                if (price != default)
                    originalOrder.SampleProduct.Price = price;
                var update = await shoppy.UpdateProduct((string)originalOrder.JSON["id"], originalOrder.JSON);
                await ctx.RespondAsync(update ? originalOrder.ToString() : "Couldn't make shoppy :/");
            }
        }
    }
}
