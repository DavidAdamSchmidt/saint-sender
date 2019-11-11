using System.Windows.Controls;
using SaintSender.Core.Entities;

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

        public DelegateCommand<string> PasswordChangedCommand { get; private set; }

        private void SetCommands()
        {
            CancelButtonClickCommand = new DelegateCommand<PasswordBox>(CancelLogin_Execute, CancelLogin_CanExecute);
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

        private void UpdateCancelLoginAvailability_Execute(string s)
        {
            CancelButtonClickCommand.RaiseCanExecuteChanged();
        }
    }
}
