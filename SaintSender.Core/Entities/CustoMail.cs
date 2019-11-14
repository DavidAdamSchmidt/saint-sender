using GemBox.Email;
using System;
using System.Linq;

namespace SaintSender.Core.Entities
{
    public class CustoMail : Base, IComparable
    {
        private bool _isRead;

        public MailAddressCollection Sender { get; set; }

        public MailAddressCollection To { get; set; }

        public string Subject { get; set; }

        public MailAddressCollection Cc { get; set; }

        public MailAddressCollection Bcc { get; set; }

        public string TextBody { get; set; }

        public string BodyHtml { get; set; }

        public bool IsRead
        {
            get => _isRead;
            set => SetProperty(ref _isRead, value);
        }

        public AttachmentCollection Attachments { get; set; }

        public int MessageNumber { get; set; }

        public DateTime Date { get; set; }

        public string From => Sender.SingleOrDefault()?.ToString();

        public int CompareTo(object other)
        {
            var compared = (CustoMail)other;
            if (compared.Date < this.Date)
            {
                return -1;
            }

            return compared.Date == Date ? 1 : 0;
        }

    }
}
