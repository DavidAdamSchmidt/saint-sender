using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;
using SaintSender.DesktopUI.Views;

namespace SaintSender.DesktopUI.ViewModels
{
    public class LoginWindowModel : Base
    {
        private string _email;
        private bool _sending;
        private readonly IList<UserInfo> _userData;

        public LoginWindowModel()
        {
            _userData = EncryptionService.RetrieveAllData();
            SetCommands();
        }

        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                CancelButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public bool IsSending
        {
            get => _sending;
            set
            {
                SetProperty(ref _sending, value);
                CancelButtonClickCommand.RaiseCanExecuteChanged();
                SignInButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public IList<string> Users => (from user in _userData select user.Email).ToList();

        public string SelectedUser { get; set; }

        public DelegateCommand<PasswordBox> CancelButtonClickCommand { get; private set; }

        public DelegateCommand<PasswordBox> SignInButtonClickCommand { get; private set; }

        public DelegateCommand<string> PasswordChangedCommand { get; private set; }

        public DelegateCommand<PasswordBox> FillLoginDetails { get; private set; }

        private void SetCommands()
        {
            CancelButtonClickCommand = new DelegateCommand<PasswordBox>(CancelLogin_Execute, CancelLogin_CanExecute);
            SignInButtonClickCommand = new DelegateCommand<PasswordBox>(AuthenticateLogin_Execute, AuthenticateLogin_CanExecute);
            PasswordChangedCommand = new DelegateCommand<string>(UpdateCancelLoginAvailability_Execute);
            FillLoginDetails = new DelegateCommand<PasswordBox>(AutoFillLoginDetails_Execute);
        }

        private void AutoFillLoginDetails_Execute(PasswordBox passwordBox)
        {
            Email = SelectedUser;

            foreach (var user in _userData)
            {
                if (!user.Email.Equals(SelectedUser))
                {
                    continue;
                }

                passwordBox.Password = user.Password;
                break;
            }
        }

        private void CancelLogin_Execute(PasswordBox passwordBox)
        {
            Email = string.Empty;
            passwordBox.Password = string.Empty;
        }

        private bool CancelLogin_CanExecute(PasswordBox passwordBox)
        {
            return (!string.IsNullOrWhiteSpace(Email) ||
                    !string.IsNullOrWhiteSpace(passwordBox?.Password)) &&
                   !_sending; ;
        }

        private async void AuthenticateLogin_Execute(PasswordBox passwordBox)
        {
            IsSending = true;

            var result = await GmailService.AuthenticateAsync(_email, passwordBox.Password);

            if (result)
            {

                var mainWindow = new MainWindow();
                mainWindow.Show();

                var parentWindow = Window.GetWindow(passwordBox);
                parentWindow?.Close();
            }
            else
            {
                IsSending = false;

                MessageBox.Show("Wrong email or password provided.", "Credentials Alert", MessageBoxButton.OK,
                    MessageBoxImage.Exclamation);
            }
        }

        private bool AuthenticateLogin_CanExecute(PasswordBox passwordBox)
        {
            return !_sending;
        }

        private void UpdateCancelLoginAvailability_Execute(string s)
        {
            CancelButtonClickCommand.RaiseCanExecuteChanged();
        }
    }
}
