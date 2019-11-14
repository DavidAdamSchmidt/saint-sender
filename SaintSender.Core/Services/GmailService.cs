using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using GemBox.Email;
using GemBox.Email.Imap;
using MailAddress = System.Net.Mail.MailAddress;
using MailMessage = System.Net.Mail.MailMessage;
using GemBoxMail = GemBox.Email.MailMessage;
using SaintSender.Core.Entities;

namespace SaintSender.Core.Services
{
    public static class GmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private const string SmtpHost = "smtp.gmail.com";
        private static readonly ImapClient ImapClient;

        public static string Email => EncryptionService.RetrieveData()[0];

        private static string Password => EncryptionService.RetrieveData()[1];

        static GmailService()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            ImapClient = new ImapClient(ImapHost);
        }

        public static async Task<bool> Authenticate(string email, string password)
        {
            return await Task.Factory.StartNew(() => TryToAuthenticate(email, password));
        }

        public static async Task<bool> SendMail(string recipient, string subject, string body)
        {
            return await Task.Factory.StartNew(() => TryToSendMail(recipient, subject, body));
        }

        public static async Task<bool> FillEmailCollection(AsyncObservableCollection<CustoMail> emails)
        {
            return await Task.Factory.StartNew(() => TryToGetEmails(emails));
        }

        private static bool TryToAuthenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }


            using (ImapClient)
            {
                ImapClient.Connect();

                try
                {
                    ImapClient.Authenticate(email, password);
                }
                catch (Exception e) when(e is InvalidOperationException || e is ArgumentException)
                {
                    return false;
                }

                if (ImapClient.IsAuthenticated)
                {
                    EncryptionService.SaveData(email, password);
                }

                return ImapClient.IsAuthenticated;
            }
        }

        private static bool TryToSendMail(string recipient, string subject, string body)
        {

            SmtpClient smtp = null;
            MailMessage message = null;

            try
            {
                var from = new MailAddress(Email);
                var to = new MailAddress(recipient);

                smtp = new SmtpClient
                {
                    Host = SmtpHost,
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from.Address, Password)
                };

                message = new MailMessage(from, to)
                {
                    Subject = subject,
                    Body = body
                };

                smtp.Send(message);

                return true;
            }
            catch (Exception ex) when(IsHandledForSendingFailure(ex))
            {
                return false;
            }
            finally
            {
                smtp?.Dispose();
                message?.Dispose();
            }
        }

        private static bool IsHandledForSendingFailure(Exception ex)
        {
            return ex is InvalidOperationException ||
                   ex is SmtpException ||
                   ex is ArgumentException ||
                   ex is FormatException;
        }

        private static bool TryToGetEmails(ICollection<CustoMail> emails)
        {
            if (string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(Password))
            {
                return false;
            }

            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Password);
                ImapClient.SelectInbox();

                var unseens = ImapClient.SearchMessageUids("Unseen");
                var seens = ImapClient.SearchMessageUids("Seen");

                try
                {
                    FillEmailCollection(unseens, emails, false);
                    FillEmailCollection(seens, emails, true);

                    return true;
                }
                catch (FreeLimitReachedException)
                {
                    return false;
                }
            }
        }

        private static void FillEmailCollection(IEnumerable<string> ids, ICollection<CustoMail> emails, bool readOrNot)
        {
            foreach (var id in ids)
            {
                var clientMail = ImapClient.GetMessage(int.Parse(id));
                var custoMail = EmailConverter(clientMail, readOrNot);
                custoMail.MessageNumber = int.Parse(id);
                emails.Add(custoMail);
            }
        }

        private static CustoMail EmailConverter(GemBoxMail clientMail, bool readOrNot)
        {
            var mail = new CustoMail
            {
                Attachments = clientMail.Attachments,
                Bcc = clientMail.Bcc,
                Cc = clientMail.Cc,
                BodyHtml = clientMail.BodyHtml,
                TextBody = clientMail.BodyText,
                Sender = clientMail.From,
                Subject = clientMail.Subject,
                To = clientMail.To,
                Date = clientMail.Date,
                IsRead = readOrNot
            };

            return mail;
        }

        public static async Task Flush(AsyncObservableCollection<CustoMail> emails)
        {
            await Task.Factory.StartNew(() => TryToFlush(emails));
        }

        private static void TryToFlush(IEnumerable<CustoMail> emails)
        {
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Password);
                ImapClient.SelectInbox();

                foreach (var mail in emails)
                {
                    if (!mail.IsRead)
                    {
                        ImapClient.SetMessageFlags(mail.MessageNumber, "Unseen");
                    }
                }
            }
        }
    }
}
