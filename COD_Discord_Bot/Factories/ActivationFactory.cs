using COD_Discord_Bot.Classes;
using COD_Discord_Bot.Exceptions;
using COD_Discord_Bot.Responses;
using MailKit;
using MailKit.Net.Imap;
using NLog;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;

namespace COD_Discord_Bot {
    class ActivationFactory {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        public Email EmailAddress { get; private set; }
        private ImapClient client;
        public ActivationFactory(Email email) {
            this.EmailAddress = email;
            client = new ImapClient();
            try {
                logger.Debug($"Logging into {EmailAddress.ToString()}");
                client.Connect("imap.mail.ru", 993, true);
                client.Authenticate(EmailAddress.Email, EmailAddress.Password);
                logger.Debug($"Logged succesfully into {EmailAddress.ToString()}");
            }
            catch (ImapProtocolException) {
                //retarded i know but had to
                throw new InvalidAccountException(EmailAddress);
            }
        }

        public async Task<CodeResponse> GetCodes(int maxCodes = 1) {
            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);
            var codes = new List<Code>();
            for (int i = inbox.Count - 1; i >= 0; i--) {
                var message = await inbox.GetMessageAsync(i);
                if (message.From.ToString().Contains("noreply@battle.net", StringComparison.OrdinalIgnoreCase)) {
                    if (message.Subject.ToString().Contains("Account Verification", StringComparison.OrdinalIgnoreCase)) {
                        if (Code.TryParse(message.Date, message.GetTextBody(MimeKit.Text.TextFormat.Html), out Code code)) {
                            codes.Add(code);
                            if (codes.Count == maxCodes) break;
                        }
                    }
                }
            }
            logger.Debug($"Total of {codes.Count} codes were found in {EmailAddress.ToString()}");
            if (codes.Count == 0) return new CodeResponse(false, "Couldn't find any codes???");
            this.client.Dispose();
            codes = codes.OrderByDescending(x => x.DateReceived).ToList();
            return new CodeResponse(true, $"Got {codes.Count} code{(codes.Count > 1 ? "s" : "")}", codes);
        }
    }
}
