using COD_Discord_Bot.Classes.Attributes;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using System;
using System.Threading.Tasks;
using System.Linq;
using COD_Discord_Bot.Utils;

namespace COD_Discord_Bot.BotTools.Commands {
    class GeneralCommands : DSharpPlus.CommandsNext.BaseCommandModule {
        [Command("resolve")]
        public async Task Resolve(CommandContext ctx, ulong id) {
            var user = await ctx.Client.GetUserAsync(id);
            await ctx.RespondAsync($"User registered on: {user.CreationTimestamp.ToReadableFormat()}" +
                $" - with username {user.Username}#{user.Discriminator}");
        }

        [Command("id")]
        public async Task Id(CommandContext ctx, DiscordMember member = default) {
            var msg = await ctx.RespondAsync(member == default ? ctx.User.Id.ToString() : member.Id.ToString());
            await Task.Delay(10000);
            await ctx.Message.DeleteAsync();
            await msg.DeleteAsync();
        }

        [Command("debug")]
        [RequireAccountsPermission]
        public async Task Debug(CommandContext ctx) {
            var embed = new DiscordEmbedBuilder();
            embed.Author = new DiscordEmbedBuilder.EmbedAuthor() { Name = "COD Bot" };
            embed.Title = "Server stats";
            var uptime = DateTime.Now - Program.dateStarted;
            embed.Description = $"**Proxies loaded:** {ActivisionCommands.proxies.Count}\n" +
                $"**Program uptime:** {uptime.FormatTimeSpan()}";
            embed.Color = DiscordColor.Blue;
            await ctx.RespondAsync(embed: embed.Build());
        }

        [Command("sum")]
        [RequireToxic]
        public async Task Sum(CommandContext ctx, int maxMsgs = 30) {
            var msgs = await ctx.Channel.GetMessagesAsync(maxMsgs);
            int sum = 0;
            foreach (var msg in msgs) {
                if (msg.Content.TryExtractNumber(out int num))
                    sum += num;
            }
            await ctx.RespondAsync($"{sum}$");
        }

        
        [Command("anounce")]
        [RequireToxic]
        public async Task Anounce(CommandContext ctx, [RemainingText] string msg) {
            int total = 0;
            foreach (var owner in ctx.Client.Guilds.Select(x => x.Value.Owner)) {
                await owner.SendMessageAsync(msg);
                total++;
            }
            await ctx.RespondAsync($"Sent to {total} servers");

        }

        private static ulong[] userIds = { 607308155373748225, 621620337733271583 };
        private static ulong[] channels = { 793888791247978527, 774774904821645322, 810685620761919567, 807040877449052162 };
        [Command("supreme")]
        public async Task Supreme(CommandContext ctx, [RemainingText] string msg) {
            if (userIds.Any(x => x.Equals(ctx.User.Id))) {
                int total = 0;
                foreach (var channel in channels) {
                    try {
                        var c = await ctx.Client.GetChannelAsync(channel);
                        await c.SendMessageAsync(msg);
                        total++;
                    } catch (Exception) {

                    }
                }
                await ctx.RespondAsync($"Sent to {total} servers");
            } else await ctx.SendThenDelete("Access denied");
        }

        [Command("kick")]
        [RequireToxic]
        public async Task Kick (CommandContext ctx, int n) {
            var members = ctx.Guild.Members.OrderByDescending(x => x.Value.JoinedAt).Select(x => x.Value).Take(n).ToList();
            await ctx.RespondAsync($"First member is {members.First().Mention} & last member is {members.Last().Mention}");
            bool confirmed = await ctx.AskConfirmation($"You sure wanna do it for {n} members??");
            if (confirmed) {
                var msg = await ctx.RespondAsync($"0/{members.Count}");
                int total = 0;
                foreach (var member in members) {
                    await member.BanAsync(100);
                    total++;
                }
                await msg.ModifyAsync($"{total}/{members.Count}");
            }
        }

        [Command("whereAreYou")]
        [RequireToxic]
        public async Task WhereAreYou(CommandContext ctx) {
            await ctx.RespondAsync($"I am at: {string.Join('\n', ctx.Client.Guilds.Select(x => x.Value.Name))}");
        }
    }
}
