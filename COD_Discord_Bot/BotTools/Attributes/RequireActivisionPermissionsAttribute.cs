using COD_Discord_Bot.DiscordClasses;
using COD_Discord_Bot.Utils;
using DSharpPlus.CommandsNext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace COD_Discord_Bot.BotTools.Attributes {
    class RequireActivisionPermissionsAttribute : DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute  {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            //toxic bypasses all
            if (Configuration.Servers.Any(x => x.Value.Any(x => x.ID.Equals(ctx.User.Id) && x.HasPermission(Permissions.Activision)))) return Task.FromResult(true);

            if (!Configuration.Servers.ContainsKey(ctx.Guild.Id)) return Task.FromResult(false);
            var user = Configuration.Servers[ctx.Guild.Id].Where(x => x.ID.Equals(ctx.User.Id)).FirstOrDefault();
            if (user == default || !user.HasPermission(Permissions.Activision)) {
                ctx.Message.DeleteAsync().GetAwaiter();
                ctx.SendThenDelete($"<@{ctx.User.Id}> This command can only be used by masters!").Wait();
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }

    }
}
