using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
using MimeKit;
using SaintSender.Core.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace SaintSender.Core.Services
{
    public static class GmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private const string SmtpHost = "smtp.gmail.com";

        public static string Email => EncryptionService.RetrieveData().Email;

        private static string Password => EncryptionService.RetrieveData().Password;

        public static async Task<bool> AuthenticateAsync(string email, string password)
        {
            return await Task.Factory.StartNew(() => Authenticate(email, password));
        }

        public static bool Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            const string domain = "@gmail.com";

            if (!email.EndsWith(domain))
            {
                email += domain;
            }

            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(ImapHost, 993, true);

            try
            {
                client.Authenticate(email, password);
            }
            catch (Exception e) when (e is AuthenticationException)
            {
                client.Dispose();

                return false;
            }

            if (client.IsAuthenticated)
            {
                EncryptionService.SaveData(email, password);
            }

            return true;
        }

        public static async Task<bool> SendAsync(string recipient, string subject, string body)
        {
            return await Task.Factory.StartNew(() => Send(recipient, subject, body));
        }

        public static bool Send(string recipient, string subject, string body)
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
            catch (Exception e) when (
                e is InvalidOperationException ||
                e is SmtpException ||
                e is ArgumentException ||
                e is FormatException)
            {
                return false;
            }
            finally
            {
                smtp?.Dispose();
                message?.Dispose();
            }
        }

        public static async Task UpdateAsync(AsyncObservableCollection<CustoMail> emails)
        {
            await Task.Factory.StartNew(() => Update(emails));
        }

        public static void Update(ICollection<CustoMail> emails)
        {
            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(ImapHost, 993, true);
            client.Authenticate(Email, Password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var items = inbox.Fetch(0, -1,
                MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);

            foreach (var item in items.Reverse())
            {
                var message = inbox.GetMessage(item.UniqueId);
                var converted = EmailConverter(message, (item.Flags & MessageFlags.Seen) != 0);

                converted.MessageNumber = item.UniqueId;
                emails.Add(converted);
            }

            client.Disconnect(true);
        }

        public static async Task<bool> SaveAsync(CustoMail mail)
        {
            return await Task.Factory.StartNew(() => Save(mail));
        }

        public static bool Save(CustoMail mail)
        {
            var currentDirectory = Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\SavedEmails");
            var fileName = string.Concat(mail.MessageNumber, mail.Subject);
            var filePath = $@"{currentDirectory.FullName}\{fileName}.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            var overwritten = !string.IsNullOrEmpty(File.ReadAllText(filePath));

            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(ImapHost, 993, true);
            client.Authenticate(Email, Password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var message = inbox.GetMessage(mail.MessageNumber);
            message.WriteTo(filePath);

            return overwritten;
        }

        public static async Task DeleteAsync(CustoMail mail)
        {
            await Task.Factory.StartNew(() => Delete(mail));
        }

        public static void Delete(CustoMail mail)
        {
            var currentDirectory = $@"{Directory.GetCurrentDirectory()}\SavedEmails";
            var files = Directory.GetFiles(currentDirectory);
            var fileName = string.Concat(mail.MessageNumber, mail.Subject);
            foreach (var file in files)
            {
                if (!file.EndsWith($@"\{fileName}.txt"))
                {
                    continue;
                }

                File.Delete(file);
                break;
            }

            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(ImapHost, 993, true);
            client.Authenticate(Email, Password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            inbox.AddFlags(new[] { mail.MessageNumber }, MessageFlags.Deleted, true);

            inbox.Expunge();
        }

        public static CustoMail EmailConverter(MimeMessage clientMail, bool readOrNot)
        {
            var mail = new CustoMail
            {
                Attachments = clientMail.Attachments,
                Bcc = clientMail.Bcc,
                Cc = clientMail.Cc,
                BodyHtml = clientMail.HtmlBody,
                TextBody = clientMail.TextBody,
                Sender = clientMail.From,
                Subject = clientMail.Subject,
                To = clientMail.To,
                Date = clientMail.Date.DateTime,
                IsRead = readOrNot
            };

            return mail;
        }
    }
}
