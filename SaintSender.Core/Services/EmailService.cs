using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using GemBox.Email;
using GemBox.Email.Imap;
using MailAddress = System.Net.Mail.MailAddress;
using MailMessage = System.Net.Mail.MailMessage;
using GemBoxMail = GemBox.Email.MailMessage;
using SaintSender.Core.Entities;

namespace SaintSender.Core.Services
{
    public static class EmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private const string SmtpHost = "smtp.gmail.com";
        private static string Pass = string.Empty;
        private static readonly ImapClient ImapClient;
        public static string Email { get; set; }

        static EmailService()
        {
            ComponentInfo.SetLicense("FREE-LIMITED-KEY");
            ImapClient = new ImapClient(ImapHost);
        }

        public static bool Authenticate(string email, string password)
        {
            if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
            {
                return false;
            }

            //TODO save credentials

            using (ImapClient)
            {
                ImapClient.Connect();

                try
                {
                    ImapClient.Authenticate(email, password);
                }
                catch (InvalidOperationException)
                {
                    return false;
                }

                if (ImapClient.IsAuthenticated)
                {
                    Email = email;
                    Pass = password;
                }

                return ImapClient.IsAuthenticated;
            }
        }

        public static async Task<bool> SendMail(string recipient, string subject, string body)
        {
            return await Task.Factory.StartNew(() => TryToSendMail(recipient, subject, body));
        }

        private static bool TryToSendMail(string recipient, string subject, string body)
        {
            // TODO: provide sender credentials

            SmtpClient smtp = null;
            MailMessage message = null;

            try
            {
                var from = new MailAddress(string.Empty);
                var to = new MailAddress(recipient);

                smtp = new SmtpClient
                {
                    Host = SmtpHost,
                    Port = 587,
                    EnableSsl = true,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Credentials = new NetworkCredential(from.Address, string.Empty)
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

        public static ObservableCollection<CustoMail> GetEmails()
        {
            var emails = new ObservableCollection<CustoMail>();
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Pass);
                ImapClient.SelectInbox();

                var unseens = ImapClient.SearchMessageUids("Unseen");
                var seens = ImapClient.SearchMessageUids("Seen");

                foreach (var mailNum in unseens)
                {
                    var clientMail = ImapClient.GetMessage(int.Parse(mailNum));
                    var custoMail = EmailConverter(clientMail, false);
                    custoMail.MessageNumber = int.Parse(mailNum);
                    emails.Add(custoMail);
                }

                foreach (var mailNum in seens)
                {
                    var clientMail = ImapClient.GetMessage(int.Parse(mailNum));
                    var custoMail = EmailConverter(clientMail, true);
                    custoMail.MessageNumber = int.Parse(mailNum);
                    emails.Add(custoMail);
                }

            }
            return emails;
        }

        private static CustoMail EmailConverter(GemBoxMail clientMail, bool readOrNot)
        {
            var mail = new CustoMail();

            mail.Attachments = clientMail.Attachments;
            mail.Bcc = clientMail.Bcc;
            mail.Cc = clientMail.Cc;
            mail.BodyHtml = clientMail.BodyHtml;
            mail.TextBody = clientMail.BodyText;
            mail.Sender = clientMail.From;
            mail.Subject = clientMail.Subject;
            mail.To = clientMail.To;
            mail.Date = clientMail.Date;

            mail.IsRead = readOrNot;
            return mail;
        }

        public static void Flush(ObservableCollection<CustoMail> Emails)
        {
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, Pass);
                ImapClient.SelectInbox();

                foreach (var mail in Emails)
                {
                    if (!mail.IsRead)
                    {
                    ImapClient.SetMessageFlags(mail.MessageNumber, new string[] { "Unseen" });
                    }
                }
            }
        }
    }
}
