using SaintSender.Core.Entities;
using SaintSender.Core.Services;

namespace SaintSender.DesktopUI.ViewModels
{
    public class ViewModelBase : ObservableBase
    {
        public ViewModelBase()
        {
            EmailService = new EmailService("gmail.com");
        }

        public ViewModelBase(EmailService emailService)
        {
            EmailService = emailService;
        }

        protected EmailService EmailService { get; }
    }
}
