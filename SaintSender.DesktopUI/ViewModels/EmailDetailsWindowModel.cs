using System;
using SaintSender.Core.Entities;
using System.Windows;
using System.Windows.Controls;
using GemBox.Email;

namespace SaintSender.DesktopUI.ViewModels
{
    public class EmailDetailsWindowModel : ViewModelBase
    {
        private MailMessage _email;

        public DelegateCommand<Button> CloseButtonClickCommand { get; private set; }

        public EmailDetailsWindowModel()
        {
            SetCommands();
            SetTestEmail();
        }

        public MailMessage Email
        {
            get => _email;
            set => _email = value;
        }

        private void SetCommands()
        {
            CloseButtonClickCommand = new DelegateCommand<Button>(CloseEmailDetailsWindow_Execute);
        }

        private void CloseEmailDetailsWindow_Execute(Button button)
        {
            var parentWindow = Window.GetWindow(button);
            parentWindow?.Close();
        }

        private void SetTestEmail()
        {
            var ms = new MailMessage(new MailAddress("testttttttttttttttttttttttttttttttt@gmail.com"))
            {
                Sender = new MailAddress("testttttttttttttttttttttttttttttttt@gmail.com"),
                Subject = "Test",
                BodyText =
                    "This is a test message pamparampapam \n\n\n\n\n\n\n\ntest\n\n\n\ntest\n\n\n\n\n\ntest\n\ntest\n\ntest",
                Date = new DateTime(2011, 08, 03, 11,12, 12)
            };
            Email = ms;
        }
    }
}
