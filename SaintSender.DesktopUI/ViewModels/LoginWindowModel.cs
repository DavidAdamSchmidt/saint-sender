using System.Windows;
using System.Windows.Controls;
using SaintSender.Core.Entities;
using SaintSender.Core.Services;

namespace SaintSender.DesktopUI.ViewModels
{
    public class LoginWindowModel : ViewModelBase
    {
        private string _email;

        public LoginWindowModel()
        {
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

        public DelegateCommand<PasswordBox> CancelButtonClickCommand { get; private set; }

        public DelegateCommand<PasswordBox> SignInButtonClickCommand { get; private set; }

        public DelegateCommand<string> PasswordChangedCommand { get; private set; }

        private void SetCommands()
        {
            CancelButtonClickCommand = new DelegateCommand<PasswordBox>(CancelLogin_Execute, CancelLogin_CanExecute);
            SignInButtonClickCommand = new DelegateCommand<PasswordBox>(AuthenticateLogin_Execute);
            PasswordChangedCommand = new DelegateCommand<string>(UpdateCancelLoginAvailability_Execute);
        }

        private void CancelLogin_Execute(PasswordBox passwordBox)
        {
            Email = string.Empty;
            passwordBox.Password = string.Empty;
        }

        private bool CancelLogin_CanExecute(PasswordBox passwordBox)
        {
            return !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(passwordBox?.Password);
        }

        private void AuthenticateLogin_Execute(PasswordBox passwordBox)
        {
            var result = EmailService.Authenticate(_email, passwordBox.Password);

            MessageBox.Show(result.ToString());
        }

        private void UpdateCancelLoginAvailability_Execute(string s)
        {
            CancelButtonClickCommand.RaiseCanExecuteChanged();
        }
    }
}
