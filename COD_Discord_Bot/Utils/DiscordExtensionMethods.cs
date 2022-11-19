using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DSharpPlus.Interactivity.Extensions;
namespace COD_Discord_Bot.Utils {
    public static class DiscordExtensionMethods {
        public static async Task SendThenDelete(this CommandContext ctx, string msg) {
            var _msg = await ctx.RespondAsync(msg);
            await Task.Delay(DiscordBotConfig.DeleteMessageDelay);
            await _msg.DeleteAsync();
        }

        private static Dictionary<ulong, bool> CachedStaff = new Dictionary<ulong, bool>();
        public static bool IsStaff(this DiscordMember member) {
            if (member.IsOwner) return true;
            if (!CachedStaff.ContainsKey(member.Id))
                CachedStaff.Add(member.Id, member.Roles
               .Select(x => x.Permissions)
               .Any(x => x.HasPermission(Permissions.Administrator) || x.HasPermission(Permissions.ManageMessages)));

            return CachedStaff[member.Id];
        }

        public static bool IsStaff(this DiscordUser member) {
            var _member = (DiscordMember)member;
            return _member.IsStaff();
        }

        public static async Task<bool> AskConfirmation(this CommandContext ctx, string message) {
            var msg = await ctx.RespondAsync(message);
            var interactivity = ctx.Client.GetInteractivity();
            bool confirmed = (await interactivity.WaitForMessageAsync(x => x.Author.Equals(ctx.User) && x.Channel.Equals(ctx.Channel)
                    && x.Content.Contains("y", System.StringComparison.OrdinalIgnoreCase) ||
                    x.Content.Contains("n", System.StringComparison.OrdinalIgnoreCase))).Result.Content.Contains("y", System.StringComparison.OrdinalIgnoreCase);
            if (!confirmed) {
                await ctx.RespondAsync("Mission cancelled!");
            }
                
            return confirmed;
        }
    }
}
