using COD_Discord_Bot.DiscordClasses;
using COD_Discord_Bot.Utils;
using DSharpPlus.CommandsNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Classes.Attributes {
    class RequireAccountsPermissionAttribute : DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute {

        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            //toxic bypasses all
            if (Configuration.Servers.Any(x => x.Value.Any(x =>  x.ID.Equals(ctx.User.Id) && x.HasPermission(Permissions.Accounts)))) return Task.FromResult(true);

            if (!Configuration.Servers.ContainsKey(ctx.Guild.Id)) return Task.FromResult(false);
            var user = Configuration.Servers[ctx.Guild.Id].Where(x => x.ID.Equals(ctx.User.Id)).FirstOrDefault();
            if (user == default || !user.HasPermission(Permissions.Accounts)) {
                ctx.Message.DeleteAsync().GetAwaiter();
                ctx.SendThenDelete($"<@{ctx.User.Id}> This command can only be used by masters!").Wait();
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }
}
