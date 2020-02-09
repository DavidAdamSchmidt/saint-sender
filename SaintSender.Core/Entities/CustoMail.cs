using MailKit;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SaintSender.Core.Entities
{
    public class CustoMail : Base, IComparable
    {
        private bool _isRead;

        public InternetAddressList Sender { get; set; }

        public InternetAddressList To { get; set; }

        public string Subject { get; set; }

        public InternetAddressList Cc { get; set; }

        public InternetAddressList Bcc { get; set; }

        public string TextBody { get; set; }

        public string BodyHtml { get; set; }

        public bool IsRead
        {
            get => _isRead;
            set => SetProperty(ref _isRead, value);
        }

        public IEnumerable<MimeEntity> Attachments { get; set; }

        public UniqueId MessageNumber { get; set; }

        public DateTime Date { get; set; }

        public string From => Sender.SingleOrDefault()?.ToString();

        public int CompareTo(object other)
        {
            var compared = (CustoMail)other;
            if (compared.Date < Date)
            {
                return -1;
            }

            return compared.Date == Date ? 0 : 1;
        }
    }
}
