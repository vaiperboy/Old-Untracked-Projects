using COD_Discord_Bot.BotTools.Attributes;
using COD_Discord_Bot.Classes;
using COD_Discord_Bot.Classes.Attributes;
using COD_Discord_Bot.Factories;
using COD_Discord_Bot.Utils;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace COD_Discord_Bot.BotTools.Commands {
    class ActivisionCommands : DSharpPlus.CommandsNext.BaseCommandModule{
        public static Proxies proxies;
        public ActivisionCommands() {
            proxies = ProxyManager.LoadProxies(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "assets", "proxies.txt"));
        }

        //[Command("stats")]
        //[RequireToxic]
        //public async Task Stats(CommandContext ctx, MessageFile file) {

        //}


        [Command("stats")]
        [RequireActivisionPermissions, RequirePrivateChannel]
        public async Task Stats(CommandContext ctx, [RemainingText]string input) {
            await ctx.RespondAsync("Feature currently paused....");
            return;
            //
            if (!input.TryParseAccounts(out List<ActivisionAccount> accounts)) {
                await ctx.RespondAsync("At least put a valid format retard?");
                return;
            }
            if (accounts.Count > Configuration.MaxActivisionAccounts
                && !Configuration.Servers[0].Any(x => x.ID.Equals(ctx.User.Id))) {
                await ctx.RespondAsync($"For now I can only do a max of {Configuration.MaxActivisionAccounts} users...");
                return;
            }
            accounts = accounts.Distinct().ToList();
            var queue = new ConcurrentQueue<ActivisionAccount>(accounts);
            var tasks = new List<Task>();
            if (accounts.Count > 1) {
                await ctx.RespondAsync(accounts.JoinAccounts());
                using (SemaphoreSlim semaphore = new SemaphoreSlim(Configuration.MaxThreads)) {
                    while (queue.TryDequeue(out var acc)) {
                        await semaphore.WaitAsync();
                        tasks.Add(Task.Run(async () => {
                            try {
                                var factory = new ActivisionFactory(acc, proxies.PickBestProxy());
                                var req = await factory.Login();
                                if (req.Success) {
                                    acc.IsWorking = await factory.GetUsernames();
                                } else acc.IsWorking = false;
                            } finally {
                                semaphore.Release();
                            }
                        }));
                    }
                    await Task.WhenAll(tasks);
                }
                await ctx.RespondAsync(accounts.JoinAccounts(false));
            } else {
                var acc = accounts.First();
                var factory = new ActivisionFactory(acc, proxies.PickBestProxy());
                var status = await ctx.RespondAsync($"**Logging into account...**");
                var req = await factory.Login();
                if (req.Success) {
                    var userReq = await factory.GetUsernames();
                    if (userReq) {
                        await status.ModifyAsync(acc.GetTrackerLink());
                    } else await status.ModifyAsync("Couldn't get stats.. '((((");
                } else await status.ModifyAsync(":red_circle: **Wrong email/pass** :red_circle:");
            }
        }
    }
}
