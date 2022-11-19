using COD_Discord_Bot.Classes.Attributes;
using COD_Discord_Bot.Utils;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace COD_Discord_Bot.BotTools.Commands {
    class ManagementCommands : DSharpPlus.CommandsNext.BaseCommandModule {

        [Command("give-role")]
        [RequireToxic]
        public async Task GiveRole(CommandContext ctx, DiscordRole role) {
            var guild = await ctx.Client.GetGuildAsync(ctx.Guild.Id);
            var users = await guild.GetAllMembersAsync();
            var hasNoRole = users
                .Where(x => x.Roles.Count() == 0 || (x.Roles.Any(f => f.Name.Contains("unverified", StringComparison.OrdinalIgnoreCase)) && x.Roles.Count() == 1))
                .ToList();
            bool confirmed = await ctx.AskConfirmation($"You sure you wanna give {role.Name} to {hasNoRole.Count} members?");
            if (!confirmed) {
                await ctx.RespondAsync("Mission cancelled..");
                return;
            }
            var msg = await ctx.RespondAsync("**Doing work...**");
            for (int i = 0; i < hasNoRole.Count; i++) {
                //if (((i / hasNoRole.Count) * 100) % 10 == 0 || i + 1 > hasNoRole.Count) {
                if (i % 15 == 0 || i + 1 > hasNoRole.Count) {
                    await msg.ModifyAsync($"**At user {i}/{hasNoRole.Count}..**");
                }
                await hasNoRole[i].GrantRoleAsync(role);
            }
            await msg.ModifyAsync($"Done ^^ {hasNoRole.Count} members...");
        }
    }
}
