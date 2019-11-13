using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
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
    }
}
