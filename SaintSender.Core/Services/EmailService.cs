using System;
using System.Collections.ObjectModel;
using GemBox.Email;
using GemBox.Email.Imap;
using SaintSender.Core.Entities;

namespace SaintSender.Core.Services
{
    public static class EmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private static string pass = string.Empty;
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
                    pass = password;
                }

                return ImapClient.IsAuthenticated;
            }
        }

        public static ObservableCollection<CustoMail> GetEmails(int offset, int limit)
        {
            var emails = new ObservableCollection<CustoMail>();
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, pass);
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

        private static CustoMail EmailConverter(MailMessage clientMail, bool readOrNot)
        {
            var mail = new CustoMail();

            mail.Attachments = clientMail.Attachments;
            mail.Bcc = clientMail.Bcc;
            mail.Cc = clientMail.Cc;
            mail.BodyHtml = clientMail.BodyHtml;
            mail.TextBody = clientMail.BodyText;
            mail.Sender = clientMail.Sender;
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
                ImapClient.Authenticate(Email, pass);
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
