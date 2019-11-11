using SaintSender.Core.Entities;

namespace SaintSender.DesktopUI.ViewModels
{
    public class LoginWindowModel : ViewModelBase
    {
        private string _email;
        private string _password;

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

        public string Password
        {
            get => _password;
            set
            {
                SetProperty(ref _password, value);
                CancelButtonClickCommand.RaiseCanExecuteChanged();
            }
        }

        public DelegateCommand<string> CancelButtonClickCommand { get; private set; }

        private void SetCommands()
        {
            SetCancelButtonCommand();
        }

        private void SetCancelButtonCommand()
        {
            CancelButtonClickCommand = new DelegateCommand<string>(CancelLogin_Execute, CancelLogin_CanExecute);
        }

        private void CancelLogin_Execute(string s)
        {
            Email = string.Empty;
            Password = string.Empty;
        }

        private bool CancelLogin_CanExecute(string s)
        {
            return !string.IsNullOrWhiteSpace(Email) || !string.IsNullOrWhiteSpace(Password);
        }
    }
}
