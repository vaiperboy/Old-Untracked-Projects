using COD_Discord_Bot.Utils;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace COD_Discord_Bot.Classes.Attributes {
    class RequireToxicAttribute : DSharpPlus.CommandsNext.Attributes.CheckBaseAttribute {
        public override Task<bool> ExecuteCheckAsync(CommandContext ctx, bool help) {
            var user = Configuration.Servers[0].Where(x => x.ID.Equals(ctx.User.Id)).FirstOrDefault();
            if (user == default) {
                ctx.SendThenDelete("You really thought you could do that? Funny....").Wait();
                return Task.FromResult(false);
            }
            return Task.FromResult(true);
        }
    }
}
