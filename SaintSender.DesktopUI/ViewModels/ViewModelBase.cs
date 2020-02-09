using SaintSender.Core.Entities;
using SaintSender.Core.Services;

namespace SaintSender.DesktopUI.ViewModels
{
    public abstract class ViewModelBase : ObservableBase
    {
        protected ViewModelBase()
            : this(new EmailService("gmail.com"))
        {
        }

        protected ViewModelBase(EmailService emailService)
        {
            EmailService = emailService;
        }

        protected EmailService EmailService { get; }
    }
}
