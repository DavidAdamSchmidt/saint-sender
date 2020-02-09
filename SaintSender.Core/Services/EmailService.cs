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
    public class EmailService
    {
        private readonly string _domain;
        private readonly string _imapHost;
        private readonly string _smtpHost;

        public EmailService(string domain)
        {
            _domain = domain;
            _imapHost = $"imap.{_domain}";
            _smtpHost = $"smtp.{_domain}";
        }

        public string Email => EncryptionService.RetrieveData().Email;

        private string Password => EncryptionService.RetrieveData().Password;

        public async Task<bool> AuthenticateAsync(string email, string password)
        {
            return await Task.Factory.StartNew(() => Authenticate(email, password));
        }

        public bool Authenticate(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (!email.EndsWith(_domain))
            {
                email += _domain;
            }

            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(_imapHost, 993, true);

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

        public async Task<bool> SendAsync(string recipient, string subject, string body)
        {
            return await Task.Factory.StartNew(() => Send(recipient, subject, body));
        }

        public bool Send(string recipient, string subject, string body)
        {
            SmtpClient smtp = null;
            MailMessage message = null;

            try
            {
                var from = new MailAddress(Email);
                var to = new MailAddress(recipient);

                smtp = new SmtpClient
                {
                    Host = _smtpHost,
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

        public async Task UpdateAsync(AsyncObservableCollection<Email> emails)
        {
            await Task.Factory.StartNew(() => Update(emails));
        }

        public void Update(ICollection<Email> emails)
        {
            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(_imapHost, 993, true);
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

        public async Task<bool> SaveAsync(Email email)
        {
            return await Task.Factory.StartNew(() => Save(email));
        }

        public bool Save(Email email)
        {
            var currentDirectory = Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\SavedEmails");
            var fileName = string.Concat(email.MessageNumber, email.Subject);
            var filePath = $@"{currentDirectory.FullName}\{fileName}.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            var overwritten = !string.IsNullOrEmpty(File.ReadAllText(filePath));

            using var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(_imapHost, 993, true);
            client.Authenticate(Email, Password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var message = inbox.GetMessage(email.MessageNumber);
            message.WriteTo(filePath);

            return overwritten;
        }

        public async Task DeleteAsync(Email email)
        {
            await Task.Factory.StartNew(() => Delete(email));
        }

        public void Delete(Email email)
        {
            var currentDirectory = $@"{Directory.GetCurrentDirectory()}\SavedEmails";
            var files = Directory.GetFiles(currentDirectory);
            var fileName = string.Concat(email.MessageNumber, email.Subject);
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

            client.Connect(_imapHost, 993, true);
            client.Authenticate(Email, Password);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            inbox.AddFlags(new[] { email.MessageNumber }, MessageFlags.Deleted, true);

            inbox.Expunge();
        }

        public Email EmailConverter(MimeMessage clientMail, bool readOrNot)
        {
            var mail = new Email
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
