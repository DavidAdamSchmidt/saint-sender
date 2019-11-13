using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using GemBox.Email;
using GemBox.Email.Imap;
using Limilabs.Client.IMAP;
using Limilabs.Mail;
using SaintSender.Core.Entities;

namespace SaintSender.Core.Services
{
    public static class EmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private static string pass = string.Empty;
        private static readonly ImapClient ImapClient;
        public static string Email { get; set; } = string.Empty;

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

        public static ObservableCollection<MailMessage> GetEmails(int offset, int limit)
        {
            var emails = new ObservableCollection<MailMessage>();
            using (ImapClient)
            {
                ImapClient.Connect();
                ImapClient.Authenticate(Email, pass);
                var folders = ImapClient.ListFolders();
                ImapClient.SelectInbox();
                var clientEmails = ImapClient.GetMessageRange(offset, limit);
                var emailInfos = ImapClient.ListMessages(offset, limit);

                for (int num = 0; num < clientEmails.Count; num++)
                {
                    var email = clientEmails[num];
                    var info = emailInfos[num];

                    var flags = ImapClient.GetMessageFlags(info.Number);

                        if (flags.Contains("Seen"))
                        {
                            email.BodyHtml.Insert(email.BodyHtml.Length, "<span hidden='hidden'>SEEN</span>");
                        }
                        else
                        {
                            email.BodyHtml.Insert(email.BodyHtml.Length, "<span hidden='hidden'>UNSEEN</span>");
                        }
                        emails.Add(email);
                }
            }
            return emails;
        }
    }
}
