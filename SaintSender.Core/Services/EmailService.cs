﻿using MailKit;
using MailKit.Net.Imap;
using MailKit.Security;
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
        private readonly EncryptionService _encryptionService;

        public EmailService(string domain)
            : this(domain, new EncryptionService())
        {
        }

        public EmailService(string domain, EncryptionService encryptionService)
        {
            _domain = domain;
            _imapHost = $"imap.{_domain}";
            _smtpHost = $"smtp.{_domain}";
            _encryptionService = encryptionService;
        }

        public static string EmailAddress { get; private set; }

        private string Password => _encryptionService.RetrievePassword(EmailAddress);

        public async Task<bool> AuthenticateAsync(string emailAddress, string password)
        {
            return await Task.Factory.StartNew(() => Authenticate(emailAddress, password));
        }

        public bool Authenticate(string emailAddress, string password)
        {
            if (string.IsNullOrWhiteSpace(emailAddress) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            if (!emailAddress.EndsWith(_domain))
            {
                emailAddress += $"@{_domain}";
            }

            using var client = SetupImapClient(false);

            try
            {
                client.Authenticate(emailAddress, password);
            }
            catch (AuthenticationException)
            {
                return false;
            }

            if (client.IsAuthenticated)
            {
                EmailAddress = emailAddress;

                _encryptionService.SaveData(emailAddress, password);
            }

            client.Disconnect(true);

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
                var from = new MailAddress(EmailAddress);
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

        public async Task UpdateAsync(AsyncObservableCollection<ObservableEmail> emails)
        {
            await Task.Factory.StartNew(() => Update(emails));
        }

        public void Update(ICollection<ObservableEmail> emails)
        {
            using var client = SetupImapClient(true);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var items = inbox.Fetch(0, -1,
                MessageSummaryItems.UniqueId | MessageSummaryItems.Size | MessageSummaryItems.Flags);

            foreach (var item in items.Reverse())
            {
                var message = inbox.GetMessage(item.UniqueId);
                var email = new ObservableEmail(message, item.UniqueId, (item.Flags & MessageFlags.Seen) != 0);

                emails.Add(email);
            }

            client.Disconnect(true);
        }

        public async Task<bool> SaveAsync(ObservableEmail email)
        {
            return await Task.Factory.StartNew(() => Save(email));
        }

        public bool Save(ObservableEmail email)
        {
            var currentDirectory = Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\SavedEmails");
            var fileName = string.Concat(email.MessageNumber, email.Subject);
            var filePath = $@"{currentDirectory.FullName}\{fileName}.txt";

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            var overwritten = !string.IsNullOrEmpty(File.ReadAllText(filePath));

            using var client = SetupImapClient(true);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadOnly);

            var message = inbox.GetMessage(email.MessageNumber);
            message.WriteTo(filePath);

            client.Disconnect(true);

            return overwritten;
        }

        public async Task DeleteAsync(ObservableEmail email)
        {
            await Task.Factory.StartNew(() => Delete(email));
        }

        public void Delete(ObservableEmail email)
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

            using var client = SetupImapClient(true);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            inbox.AddFlags(new[] { email.MessageNumber }, MessageFlags.Deleted, true);
            inbox.Expunge();

            client.Disconnect(true);
        }

        public async Task MarkAsReadAsync(ObservableEmail email)
        {
            await Task.Factory.StartNew(() => MarkAsRead(email));
        }

        public void MarkAsRead(ObservableEmail email)
        {
            using var client = SetupImapClient(true);

            var inbox = client.Inbox;
            inbox.Open(FolderAccess.ReadWrite);
            inbox.AddFlags(new[] { email.MessageNumber }, MessageFlags.Seen, true);

            client.Disconnect(true);
        }

        private ImapClient SetupImapClient(bool authenticate)
        {
            var client = new ImapClient { ServerCertificateValidationCallback = (s, c, h, e) => true };

            client.Connect(_imapHost, 993, true);

            if (authenticate)
            {
                client.Authenticate(EmailAddress, Password);
            }

            return client;
        }
    }
}
