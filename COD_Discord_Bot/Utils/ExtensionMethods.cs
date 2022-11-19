using COD_Discord_Bot.Classes;
using DSharpPlus.Entities;
using Newtonsoft.Json.Linq;
using Org.BouncyCastle.Math.EC.Rfc7748;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.RegularExpressions;

namespace COD_Discord_Bot {
    public static class ExtensionMethods {

        public static string JoinAccounts(this List<ActivisionAccount> accounts, bool showEmails = true) {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < accounts.Count; i++) {
                var acc = accounts[i];
                sb.Append($"**{i + 1}** ");
                if (!acc.IsWorking) {
                    sb.AppendLine("- :red_circle: ");
                    continue;
                }
                if (showEmails)
                    sb.Append($"- {acc.Email}:{acc.Password} ");
                string link = acc.GetTrackerLink();
                if (!string.IsNullOrEmpty(link))
                    sb.Append($"- {link}");
                sb.AppendLine();
            }
            return sb.ToString();
        }

        public static string WrapInTripleQuotes(this string msg)
            => $"```{msg}```";

        public static string ToReadableFormat(this DateTimeOffset date)
            => date.ToString("MM/dd/yyyy HH:mm:ss");

        public static string ToReadableFormat(this DateTime date)
           => date.ToString("MM/dd/yyyy HH:mm:ss");

        private static Regex emailRegex = new Regex(@"^(\S+)@(\S+)\.(\S+)$");

        public static bool IsValidEmail(this string email)
            => emailRegex.IsMatch(email);

        private static readonly Regex accountRegex = new Regex(@"([^@\s]+@[^:\s]+):(\S+)");
        public static bool TryParseAccounts(this string input, out List<ActivisionAccount> emails) {
            var matches = accountRegex.Matches(input);
            emails = new List<ActivisionAccount>();
            if (matches.Count == 0) return false;
            for (int i = 0; i < matches.Count; i++)
                emails.Add(new ActivisionAccount(matches[i].Groups[1].Value, matches[i].Groups[2].Value));
            return true;
        }

        public static bool TryParseEmails(this string input, out List<string> emails) {
            emails = accountRegex.Matches(input).Select(x => x.Value).ToList();
            return emails.Count > 0;
        }

        private static readonly Regex dollarExtractRegex = new Regex(@"(\(|\s+)(\d+)[$]");
        public static bool TryExtractNumber(this string input, out int number) {
            var matches = dollarExtractRegex.Matches(input);
            number = 0;
            if (matches.Count != 1) return false;
            number = int.Parse(matches.First().Groups[2].Value);
            return true;
        }

        private static readonly Regex productIdRegex = new Regex(@"[^@\s]{7}$");
        public static bool TryParseProductId(this string[] input, out List<string> orderIds) {
            orderIds = new List<string>();
            foreach (var s in input) {
                var matches = productIdRegex.Matches(s);
                if (matches.Count == 1)
                    orderIds.Add(matches.First().Value);
            }
            return orderIds.Count > 0;
        }

        public static string ReplaceWithEntities(this string value)
            => value.Trim().Replace("#", "%23").Replace(" ", "%20");

        public static Stream AsStream(this string input)
            => new MemoryStream(Encoding.UTF8.GetBytes(input));

        private static Regex orderRegex = new Regex(@"\w{8}-\w{4}-\w{4}-\w{4}-\w{12}");
        public static bool IsValidOrderId(this string input)
            => orderRegex.IsMatch(input.Trim());

        public static bool TryParseOrderId(this string input, out string code) {
            var matches = orderRegex.Matches(input);
            code = string.Empty;
            if (matches.Count != 1) return false;
            code = matches.First().Value.Trim();
            return true;
        }

        public static DiscordEmbedBuilder.EmbedAuthor AsEmbedAuthor(this DiscordUser user)
            => new DiscordEmbedBuilder.EmbedAuthor() {
                Name = user.Username,
                IconUrl = user.AvatarUrl
            };

        public static bool ContainsWholeWord(this string input, string word)
            => input.Split(' ').Any(x => x.Equals(word, StringComparison.OrdinalIgnoreCase)); 

        public static string ReplaceWholeWord(this string input, string oldR, string newR) {
            var output = string.Empty;
            foreach (var split in input.Split(' ')) {
                output += split.Equals(oldR, StringComparison.OrdinalIgnoreCase) ? newR : split;
                output += " ";
            }
            return output;
        }

        public static List<string> ParseAccounts(this JArray accounts) {
            var output = new List<string>();
            for (int i = 0; i < accounts.Count; i++) {
                output.Add((string)accounts[i]["account"]);
            }
            return output;
        }
        
        public static JArray ToJArray(this List<string> accounts) {
            var output = new JArray();
            for (int i = 0; i < accounts.Count; i++) {
                dynamic obj = new JObject();
                obj.account = accounts[i];
                output.Add(obj);
            }
            return output;
        }

        public static string FormatTimeSpan(this TimeSpan time) {
            var format = "G" + 3;
            var msg = time.TotalMilliseconds < 1000 ? time.TotalMilliseconds.ToString(format) + " milliseconds"
                : (time.TotalSeconds < 60 ? time.TotalSeconds.ToString(format) + " seconds"
                    : (time.TotalMinutes < 60 ? time.TotalMinutes.ToString(format) + " minutes"
                        : (time.TotalHours < 24 ? time.TotalHours.ToString(format) + " hours"
                                                : time.TotalDays.ToString(format) + " days")));
            msg += " ago";
            return msg;
        }
    }
}
