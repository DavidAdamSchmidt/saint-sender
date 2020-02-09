using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using System.Windows;
using System.Windows.Controls;

namespace SaintSender.DesktopUI.ViewModels
{
    public class ComposeWindowModel : Base
    {
        private string _recipient;
        private string _subject;
        private string _message;
        private bool _sending;

        public ComposeWindowModel()
        {
            SetCommands();
        }

        public string Recipient
        {
            get => _recipient;
            set => SetProperty(ref _recipient, value);
        }

        public string Subject
        {
            get => _subject;
            set => SetProperty(ref _subject, value);
        }

        public string Message
        {
            get => _message;
            set => SetProperty(ref _message, value);
        }

        public bool IsSending
        {
            get => _sending;
            set => SetProperty(ref _sending, value);
        }

        public DelegateCommand<Button> SendButtonClickCommand { get; private set; }

        public DelegateCommand<string> CancelButtonClickCommand { get; private set; }

        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            SendButtonClickCommand.RaiseCanExecuteChanged();
            CancelButtonClickCommand.RaiseCanExecuteChanged();
        }

        private void SetCommands()
        {
            SendButtonClickCommand = new DelegateCommand<Button>(SendEmail_Execute, SendEmail_CanExecute);
            CancelButtonClickCommand = new DelegateCommand<string>(CancelInput_Execute, CancelInput_CanExecute);
        }

        private async void SendEmail_Execute(Button button)
        {
            IsSending = true;

            var sent = await GmailService.SendAsync(_recipient, _subject, _message);

            IsSending = false;

            if (sent)
            {
                MessageBox.Show($"Your e-mail has bent sent to {_recipient}", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                var window = Window.GetWindow(button);
                window?.Close();
            }
            else
            {
                MessageBox.Show($"Couldn't send your e-mail to {_recipient}", "Failed Operation",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool SendEmail_CanExecute(Button button)
        {
            return !string.IsNullOrWhiteSpace(_recipient) &&
                   !string.IsNullOrWhiteSpace(_subject) &&
                   !string.IsNullOrWhiteSpace(_message) &&
                   !_sending;
        }

        private void CancelInput_Execute(string s)
        {
            Recipient = string.Empty;
            Subject = string.Empty;
            Message = string.Empty;
        }

        private bool CancelInput_CanExecute(string s)
        {
            return (!string.IsNullOrWhiteSpace(_recipient) ||
                   !string.IsNullOrWhiteSpace(_subject) ||
                   !string.IsNullOrWhiteSpace(_message)) &&
                   !_sending;
        }
    }
}
