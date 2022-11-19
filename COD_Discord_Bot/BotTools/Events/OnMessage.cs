using COD_Discord_Bot.Utils;
using DSharpPlus;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.BotTools.Events {
    class OnMessage {
        public static async Task CheckForCensoring(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e, Dictionary<string[], string> CensoredWords) {
            if (e.Guild != default && Configuration.CensoredServersID.Contains(e.Guild.Id)) {
                bool isStaff = (await e.Guild.GetMemberAsync(e.Author.Id)).IsStaff();
                if (!isStaff) {
                    var msg = e.Message;
                    var resultStringSplit = msg.Content.Split(' ');
                    for (int i = 0; i < resultStringSplit.Length; i++) {
                        var word = resultStringSplit[i];
                        foreach (var keyValue in CensoredWords) {
                            if (keyValue.Key.Any(x => x.Equals(word, StringComparison.OrdinalIgnoreCase))) {
                                word = keyValue.Value;
                                break;
                            }
                        }
                        resultStringSplit[i] = word;
                    }
                    var resultString = string.Join(' ', resultStringSplit);
                    if (!resultString.Equals(msg.Content)) {
                        await e.Message.DeleteAsync();
                        var embed = new DiscordEmbedBuilder();
                        embed.Author = e.Author.AsEmbedAuthor();
                        embed.Description = resultString;
                        await e.Message.RespondAsync(embed: embed.Build());
                    }
                }
            }
            return;
        }

        private static List<ulong> SentOrders = new List<ulong>();


        public async static Task ScanForOrderID(DiscordClient sender, DSharpPlus.EventArgs.MessageCreateEventArgs e) {
            //restriction
            //var shoppy = Configuration.Shoppys[(ulong)ServerIds.CodAccounts];
            var shoppy = Configuration.Shoppys[e.Guild.Id];
            if (e.Message.Content.StartsWith("."))
                return;
            if (e.Guild != default) {
                if (SentOrders.Contains(e.Channel.Id) && !e.Author.IsStaff()) return;
                if (Enum.IsDefined(typeof(TicketsChannelIds), e.Channel.ParentId)) {
                    var msg = e.Message.Content;
                    var channel = e.Channel;
                    if (msg.TryParseOrderId(out string orderId)) {
                        var accounts = await shoppy.GetOrder(orderId);
                        if (accounts.Success) {
                            await channel.SendMessageAsync(embed: DiscordHelper.CreateEmbed("OrderHelper", accounts.ToString()));
                            await channel.SendFileAsync("account.txt", string.Join('\n', accounts.Products).AsStream());
                        }
                        SentOrders.Add(e.Channel.Id);
                    }
                }
            }
            return;
        }
    }
}
