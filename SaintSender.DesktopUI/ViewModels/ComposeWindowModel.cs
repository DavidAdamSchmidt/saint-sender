using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using System.Windows;

namespace SaintSender.DesktopUI.ViewModels
{
    public class ComposeWindowModel : ViewModelBase
    {
        private string _recipient;
        private string _subject;
        private string _message;

        public ComposeWindowModel()
        {
            SetCommands();
        }

        public string Recipient
        {
            get => _recipient;
            set => SetEmailProperty(ref _recipient, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetEmailProperty(ref _subject, value);
        }

        public string Message
        {
            get => _message;
            set => SetEmailProperty(ref _message, value);
        }

        public DelegateCommand<string> SendButtonClickCommand { get; private set; }

        private void SetCommands()
        {
            SendButtonClickCommand = new DelegateCommand<string>(SendEmail_Execute, SendEmail_CanExecute);
        }

        private void SetEmailProperty(ref string storage, string value)
        {
            storage = value;
            SendButtonClickCommand.RaiseCanExecuteChanged();
        }

        private async void SendEmail_Execute(string s)
        {
            var sent = await EmailService.SendMail("", _subject, _message);

            if (sent)
            {
                MessageBox.Show($"Your e-mail has bent sent to {_recipient}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show($"Couldn't send your e-mail to {_recipient}", "Failed Operation",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SendEmail_CanExecute(string s)
        {
            return !string.IsNullOrWhiteSpace(_recipient) &&
                   !string.IsNullOrWhiteSpace(_subject) &&
                   !string.IsNullOrWhiteSpace(_message);
        }
    }
}
