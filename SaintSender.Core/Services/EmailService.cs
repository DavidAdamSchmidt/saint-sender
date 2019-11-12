using System;
using System.Net;
using System.Net.Mail;
using GemBox.Email;
using GemBox.Email.Imap;
using MailAddress = System.Net.Mail.MailAddress;
using MailMessage = System.Net.Mail.MailMessage;

namespace SaintSender.Core.Services
{
    public static class EmailService
    {
        private const string ImapHost = "imap.gmail.com";
        private const string SmtpHost = "smtp.gmail.com";
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

        public static void SendMail(string recipient, string subject, string body)
        {
            // TODO: provide sender credential

            var from = new MailAddress(string.Empty);
            var to = new MailAddress(recipient);

            var smtp = new SmtpClient
            {
                Host = SmtpHost,
                Port = 587,
                EnableSsl = true,
                DeliveryMethod = SmtpDeliveryMethod.Network,
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential(from.Address, string.Empty)
            };

            var message = new MailMessage(from, to)
            {
                Subject = subject,
                Body = body
            };

            smtp.Send(message);
            smtp.Dispose();
        }
    }
}
