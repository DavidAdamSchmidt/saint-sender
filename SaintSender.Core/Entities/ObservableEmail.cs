using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintSender.Core.Entities
{
    public class ObservableEmail : ObservableBase, IComparable
    {
        private bool _isRead;

        public InternetAddressList From { get; set; }

        public InternetAddressList To { get; set; }

        public string Subject { get; set; }

        public InternetAddressList Cc { get; set; }

        public InternetAddressList Bcc { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        public bool IsRead
        {
            get => _isRead;
            set => SetProperty(ref _isRead, value);
        }

        public IEnumerable<MimeEntity> Attachments { get; set; }

        public UniqueId MessageNumber { get; set; }

        public DateTime Date { get; set; }

        public string FirstSender => From.SingleOrDefault()?.ToString();

        public static ObservableEmail ConvertFromMimeMessage(MimeMessage message, bool isRead)
        {
            return new ObservableEmail
            {
                Attachments = message.Attachments,
                Bcc = message.Bcc,
                Cc = message.Cc,
                HtmlBody = message.HtmlBody,
                TextBody = message.TextBody,
                From = message.From,
                Subject = message.Subject,
                To = message.To,
                Date = message.Date.DateTime,
                IsRead = isRead
            };
        }

        public int CompareTo(object other)
        {
            var compared = (ObservableEmail)other;
            if (compared.Date < Date)
            {
                return -1;
            }

            return compared.Date == Date ? 0 : 1;
        }
    }
}
