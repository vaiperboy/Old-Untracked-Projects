using COD_Discord_Bot.DiscordClasses;
using COD_Discord_Bot.Utils;
using DSharpPlus.CommandsNext;
using System.Linq;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Classes.Attributes {
    class RequireShoppyPermissionsAttribute : DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            //toxic bypasses all
            if (Configuration.Servers[0].Any(x => x.ID.Equals(ctx.User.Id) && x.HasPermission(Permissions.Shoppy))) return Task.FromResult(true);
            if (!Configuration.Servers.ContainsKey(ctx.Guild.Id)) return Task.FromResult(false);
            var user = Configuration.Servers[ctx.Guild.Id].Where(x => x.ID.Equals(ctx.User.Id)).FirstOrDefault();
            if (user == default || !user.HasPermission(Permissions.Shoppy)) {
                ctx.Message.DeleteAsync().GetAwaiter();
                ctx.SendThenDelete($"<@{ctx.User.Id}> This command can only be used by masters!").Wait();
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }
}
