using COD_Discord_Bot.Classes;
using COD_Discord_Bot.Classes.Attributes;
using COD_Discord_Bot.Exceptions;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using NLog;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using COD_Discord_Bot.Utils;
using System;
using DSharpPlus.Interactivity.Extensions;
using System.Collections.Generic;

namespace COD_Discord_Bot {
    public partial class CODCommands : DSharpPlus.CommandsNext.BaseCommandModule {
        
        private readonly Logger logger = LogManager.GetCurrentClassLogger();
        private static AccountsIO writer = new AccountsIO(Configuration.AccountsLocation);

        [Command("code")]
        [RequirePrivateChannel]
        [Description("This is used to get the verfication code faster rather than manually logging into the email. Input the email that you received in your email (the mail.ru part) in that SAME EXACT FORMAT. **e.g: .code email@mail.ru:password123**")]
        public async Task Code(CommandContext ctx, [RemainingText] string input) {
            if (!input.TryParseAccounts(out List<ActivisionAccount> accounts)) {
                await ctx.RespondAsync("Format should be in email:password only! (the part where you received mail.ru)");
                return;
            }
            if (accounts.Count > 1) {
                await ctx.RespondAsync("For now I can only do a max of 1 user...");
                return;
            }
            var acc = accounts.First();
            var emailAddress = new Email(acc.Email, acc.Password);
            if (Configuration.AllowedTLDS.Contains(emailAddress.TLD)) {
                var toEdit = await ctx.RespondAsync("**Logging into account...**");
                await ctx.TriggerTypingAsync();
                try {
                    logger.Debug($"Code request initiated by {ctx.User.Username}#{ctx.User.Discriminator} for {emailAddress}");
                    var factory = new ActivationFactory(emailAddress);
                    var codes = await factory.GetCodes(Configuration.MaxCodesPerEmail);
                    if (codes.Success) {
                        await toEdit.ModifyAsync(
                            "",
                            DiscordHelper.CreateEmbed(codes.Message, string.Join('\n',
                            codes.Codes
                            .Select(x => $"**{x.CodeValue}** - Received on {x.DateReceived.ToReadableFormat()}")
                            .ToList())));
                    } else await toEdit.ModifyAsync("", DiscordHelper.CreateEmbed("Found no codes???", error: true));
                }
                catch (InvalidAccountException) {
                    await toEdit.ModifyAsync("", DiscordHelper.CreateEmbed("Failed to login to the account! Make sure info is correct\n" +
                        "**(It's also possible the account is working fine but I am malfunctioning. If so, just use the security question method)**", error: true));
                }
            } else await ctx.RespondAsync($"Allowed domains are only {string.Join(", ", Configuration.AllowedTLDS)}");
             
        }

        [Command("why-us")]
        [RequireAccountsPermission]
        public async Task WhyUs(CommandContext ctx) {
            var text = await File.ReadAllTextAsync(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "why-us.txt"));
            var builder = new DiscordEmbedBuilder();
            builder.Author = new DiscordEmbedBuilder.EmbedAuthor() {
                Name = "COD Accounts"
            };
            builder.Title = "Why buy from us?";
            builder.Description = text;
            builder.Color = DiscordColor.PhthaloBlue;
            await ctx.RespondAsync("", embed: builder.Build());
        }


        [Command("give")]
        [RequireAccountsPermission]
        public async Task Give(CommandContext ctx, DiscordMember member, int amount) {
            if (amount <= 0) {
                await ctx.RespondAsync("Retard");
                return;
            }
            bool confirmed = true;
            if (amount <= writer.Count) {
                if (amount > Configuration.SafeAmountToSend) {
                    confirmed = await ctx.AskConfirmation($"You're sending {amount} accounts to <@{member.Id}>. You sure?");
                }
                if (confirmed) {
                    var emails = await writer.GetEmail(amount, false);
                    if (emails.IsSucess) {
                        await member.SendMessageAsync($"You received {amount} account(s) from my boss");
                        await member.SendFileAsync("account.txt", emails.Message.AsStream());
                        logger.Info($"{ctx.User.Mention} Sent {amount} accounts to {member.Username}#{member.Discriminator}. \n{emails.Message}");
                        _ = await writer.GetEmail(amount, true);
                        await ctx.RespondAsync($"{ctx.User.Username}#{ctx.User.Discriminator} sent {amount} account{(amount > 1 ? "s" : "")} to <@{member.Id}>. Available stock: {writer.Count}");
                    } else await ctx.RespondAsync($"Couldn't send accounts: {emails.Message}");
                } 
            } else await ctx.RespondAsync($"Not enough in stock! Current stock {writer.Count}");
        }

        [Command("stock")]
        [RequireAccountsPermission]
        public async Task Stock(CommandContext ctx) {
            await ctx.RespondAsync($"Current stock: {writer.Count}");
        }
    }
}
