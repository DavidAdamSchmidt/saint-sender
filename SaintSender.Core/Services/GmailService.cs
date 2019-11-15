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
using System.IO;

namespace SaintSender.Core.Services
{
    public static class GmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private const string SmtpHost = "smtp.gmail.com";
        private static readonly ImapClient ImapClient;
        private static readonly List<CustoMail> Backup = new List<CustoMail>();

        public static string Email => EncryptionService.RetrieveData().Email;

        private static string Password => EncryptionService.RetrieveData().Password;

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
            try
            {
                return await Task.Factory.StartNew(() => TryToFillEmailCollection(emails));
            }
            catch (FreeLimitReachedException)
            {
                await Flush(Backup);

                Backup.Clear();

                return false;
            }
        }

        public static async Task<bool> SaveEmailToFile(CustoMail mail)
        {
            return await Task.Factory.StartNew(() => TryToSaveEmailToFile(mail));
        }

        public static async Task DeleteEmail(CustoMail mail)
        {
            await Task.Factory.StartNew(() => TryToDeleteEmail(mail));
        }

        public static async Task Flush(IEnumerable<CustoMail> emails)
        {
            await Task.Factory.StartNew(() => TryToFlush(emails));
        }

        private static bool TryToAuthenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            const string domain = "@gmail.com";

            if (!email.EndsWith(domain))
            {
                email += domain;
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

        private static bool TryToFillEmailCollection(ICollection<CustoMail> emails)
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

                FillEmailCollectionByIds(unseens, false);
                FillEmailCollectionByIds(seens, true);
                Backup.Sort();
                Backup.ForEach(emails.Add);
                Backup.Clear();
                return true;
            }
        }

        private static bool TryToSaveEmailToFile(CustoMail mail)
        {
            var currentDirectory = Directory.CreateDirectory($@"{Directory.GetCurrentDirectory()}\SavedEmails");
            var fileName = string.Concat(mail.MessageNumber, mail.Subject);
            var filePath = $@"{currentDirectory.FullName}\{fileName}.txt";
            if (!File.Exists(filePath))
            {
                File.Create(filePath).Dispose();
            }

            var overwritten = !string.IsNullOrEmpty(File.ReadAllText(filePath));

            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Password);
                ImapClient.SelectInbox();
                ImapClient.SaveMessage(mail.MessageNumber, filePath);
            }

            return overwritten;
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

        private static void TryToDeleteEmail(CustoMail mail)
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
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Password);
                ImapClient.SelectInbox();
                ImapClient.DeleteMessage(mail.MessageNumber, true);
            }
        }

        private static void FillEmailCollectionByIds(IEnumerable<string> ids, bool readOrNot)
        {
            foreach (var id in ids)
            {
                var clientMail = ImapClient.GetMessage(id);
                var custoMail = EmailConverter(clientMail, readOrNot);
                custoMail.MessageNumber = int.Parse(id);
                Backup.Add(custoMail);
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

        private static bool IsHandledForSendingFailure(Exception ex)
        {
            return ex is InvalidOperationException ||
                   ex is SmtpException ||
                   ex is ArgumentException ||
                   ex is FormatException;
        }
    }
}
