using COD_Discord_Bot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Classes.Attributes {
    class RequirePrivateChannelAttribute : DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            if (ctx.Channel.IsPrivate) 
                return Task.FromResult(true);
            var users = ctx.Channel.Users;
            int nonStaffCount = 0;
            foreach (var user in users) {
                if (user.IsBot) continue;
                else if (user.IsStaff()) continue;
                else nonStaffCount++;
                if (nonStaffCount > 1) {
                    ctx.Message.DeleteAsync().GetAwaiter();
                    ctx.SendThenDelete($"<@{ctx.Member.Id}> other people are seeing this message! DM me or go to a more private channel").Wait();
                    return Task.FromResult(false);
                }
            }
            return Task.FromResult(true);
        }
    }
}
