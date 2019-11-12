using System;
using GemBox.Email;
using GemBox.Email.Imap;

namespace SaintSender.Core.Services
{
    public static class EmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private static readonly ImapClient ImapClient;

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

                return ImapClient.IsAuthenticated;
            }
        }
    }
}
