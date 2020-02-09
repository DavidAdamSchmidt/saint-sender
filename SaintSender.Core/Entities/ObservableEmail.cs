using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintSender.Core.Entities
{
    public class ObservableEmail : ObservableBase, IComparable<ObservableEmail>
    {
        private bool _isRead;

        public ObservableEmail()
        {
        }

        public ObservableEmail(MimeMessage message, UniqueId messageNumber, bool isRead)
        {
            From = message.From;
            To = message.To;
            Subject = message.Subject;
            Bcc = message.Bcc;
            Cc = message.Cc;
            TextBody = message.TextBody;
            HtmlBody = message.HtmlBody;
            Attachments = message.Attachments;
            Date = message.Date.DateTime;
            MessageNumber = messageNumber;
            IsRead = isRead;
        }

        public InternetAddressList From { get; set; }

        public InternetAddressList To { get; set; }

        public string Subject { get; set; }

        public InternetAddressList Cc { get; set; }

        public InternetAddressList Bcc { get; set; }

        public string TextBody { get; set; }

        public string HtmlBody { get; set; }

        public IEnumerable<MimeEntity> Attachments { get; set; }

        public DateTime Date { get; set; }

        public UniqueId MessageNumber { get; set; }

        public bool IsRead
        {
            get => _isRead;
            set => SetProperty(ref _isRead, value);
        }

        public string FirstSender => From.SingleOrDefault()?.ToString();

        public int CompareTo(ObservableEmail other)
        {
            if (Date == other.Date)
            {
                return 0;
            }

            return Date < other.Date ? -1 : 1;
        }
    }
}
