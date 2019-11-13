using GemBox.Email;
using GemBox.Email.Imap;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace SaintSender.Core.Entities
{
    public class CustoMail : Base
    {
        private bool _isRead;
        public MailAddressCollection Sender { get; set; }

        public MailAddressCollection To { get; set; }

        public string Subject { get; set; }

        public MailAddressCollection Cc { get; set; }

        public MailAddressCollection Bcc { get; set; }

        public string TextBody { get; set; }

        public string BodyHtml { get; set; }

        public bool IsRead { get => _isRead; set => SetProperty(ref _isRead, value); }

        public AttachmentCollection Attachments { get; set; }

        public int MessageNumber { get; set; }

        public DateTime Date { get; set; }

        public string From { get => Sender.SingleOrDefault().ToString(); }

    }
}
